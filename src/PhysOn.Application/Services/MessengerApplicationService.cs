using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PhysOn.Application.Abstractions;
using PhysOn.Application.Exceptions;
using PhysOn.Contracts.Auth;
using PhysOn.Contracts.Common;
using PhysOn.Contracts.Conversations;
using PhysOn.Domain.Accounts;
using PhysOn.Domain.Conversations;
using PhysOn.Domain.Invites;
using PhysOn.Domain.Messages;

namespace PhysOn.Application.Services;

public sealed class MessengerApplicationService
{
    private const int DefaultConversationPageSize = 50;
    private const int DefaultMessagePageSize = 50;

    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly ITokenService _tokenService;
    private readonly IRealtimeNotifier _realtimeNotifier;

    public MessengerApplicationService(
        IAppDbContext db,
        IClock clock,
        ITokenService tokenService,
        IRealtimeNotifier realtimeNotifier)
    {
        _db = db;
        _clock = clock;
        _tokenService = tokenService;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<RegisterAlphaQuickResponse> RegisterAlphaQuickAsync(
        RegisterAlphaQuickRequest request,
        string wsUrl,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var normalizedDisplayName = NormalizeDisplayName(request.DisplayName);
        var inviteCode = NormalizeInviteCode(request.InviteCode);

        var invite = await _db.Invites
            .FirstOrDefaultAsync(x => x.CodeHash == HashInviteCode(inviteCode), cancellationToken)
            ?? throw InvalidInviteException();

        if (!invite.CanUse(now))
        {
            throw InvalidInviteException();
        }

        var account = new Account
        {
            Id = Guid.NewGuid(),
            DisplayName = normalizedDisplayName,
            CreatedAt = now
        };

        var device = new Device
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            InstallId = NormalizeInstallId(request.Device.InstallId),
            Platform = string.IsNullOrWhiteSpace(request.Device.Platform) ? "windows" : request.Device.Platform.Trim().ToLowerInvariant(),
            DeviceName = string.IsNullOrWhiteSpace(request.Device.DeviceName) ? "Windows PC" : request.Device.DeviceName.Trim(),
            AppVersion = string.IsNullOrWhiteSpace(request.Device.AppVersion) ? "0.1.0" : request.Device.AppVersion.Trim(),
            CreatedAt = now,
            LastSeenAt = now
        };

        var session = new Session
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            DeviceId = device.Id,
            TokenFamilyId = Guid.NewGuid(),
            CreatedAt = now,
            LastSeenAt = now
        };

        var selfConversation = new Conversation
        {
            Id = Guid.NewGuid(),
            Type = ConversationType.Self,
            CreatedByAccountId = account.Id,
            CreatedAt = now
        };

        var selfMembership = new ConversationMember
        {
            ConversationId = selfConversation.Id,
            AccountId = account.Id,
            Role = ConversationRole.Owner,
            JoinedAt = now,
            PinOrder = 0
        };

        Conversation? inviterConversation = null;
        List<ConversationMember> inviterMembers = [];

        if (invite.IssuedByAccountId is Guid inviterAccountId && inviterAccountId != account.Id)
        {
            inviterConversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Type = ConversationType.Direct,
                CreatedByAccountId = inviterAccountId,
                CreatedAt = now
            };

            inviterMembers =
            [
                new ConversationMember
                {
                    ConversationId = inviterConversation.Id,
                    AccountId = inviterAccountId,
                    Role = ConversationRole.Owner,
                    JoinedAt = now
                },
                new ConversationMember
                {
                    ConversationId = inviterConversation.Id,
                    AccountId = account.Id,
                    Role = ConversationRole.Member,
                    JoinedAt = now
                }
            ];
        }

        var issuedTokens = _tokenService.IssueTokens(account, session, device, now);
        session.RefreshTokenHash = issuedTokens.RefreshTokenHash;
        session.ExpiresAt = issuedTokens.RefreshTokenExpiresAt;

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        _db.Accounts.Add(account);
        _db.Devices.Add(device);
        _db.Sessions.Add(session);
        _db.Conversations.Add(selfConversation);
        _db.ConversationMembers.Add(selfMembership);

        if (inviterConversation is not null)
        {
            _db.Conversations.Add(inviterConversation);
            _db.ConversationMembers.AddRange(inviterMembers);
        }

        invite.UsedCount += 1;

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var bootstrap = await GetBootstrapAsync(account.Id, session.Id, wsUrl, cancellationToken);
        return new RegisterAlphaQuickResponse(
            bootstrap.Me,
            bootstrap.Session,
            new TokenPairDto(
                issuedTokens.AccessToken,
                issuedTokens.AccessTokenExpiresAt,
                issuedTokens.RefreshToken,
                issuedTokens.RefreshTokenExpiresAt),
            bootstrap);
    }

    public async Task<BootstrapResponse> GetBootstrapAsync(
        Guid accountId,
        Guid sessionId,
        string wsUrl,
        CancellationToken cancellationToken)
    {
        var account = await _db.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == accountId, cancellationToken)
            ?? throw NotFound("account_not_found", "사용자 정보를 찾을 수 없습니다.");

        var session = await _db.Sessions
            .AsNoTracking()
            .Include(x => x.Device)
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.AccountId == accountId, cancellationToken)
            ?? throw NotFound("session_not_found", "세션 정보를 찾을 수 없습니다.");

        var conversations = await ListConversationsAsync(accountId, DefaultConversationPageSize, cancellationToken);

        return new BootstrapResponse(
            ToMeDto(account),
            new SessionDto(session.Id.ToString(), session.DeviceId.ToString(), session.Device?.DeviceName ?? "Windows PC", session.CreatedAt),
            BuildBootstrapWsDto(account, session, session.Device, wsUrl),
            conversations);
    }

    public async Task<MeDto> GetMeAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var account = await _db.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == accountId, cancellationToken)
            ?? throw NotFound("account_not_found", "사용자 정보를 찾을 수 없습니다.");

        return ToMeDto(account);
    }

    private BootstrapWsDto BuildBootstrapWsDto(Account account, Session session, Device? device, string wsUrl)
    {
        var resolvedDevice = device ?? session.Device ?? throw new AppException(
            "device_not_found",
            "세션 기기 정보를 찾을 수 없습니다.",
            System.Net.HttpStatusCode.Unauthorized);

        var issuedRealtimeTicket = _tokenService.IssueRealtimeTicket(account, session, resolvedDevice, _clock.UtcNow);
        return new BootstrapWsDto(wsUrl, issuedRealtimeTicket.Token, issuedRealtimeTicket.ExpiresAt);
    }

    public async Task<ListEnvelope<ConversationSummaryDto>> ListConversationsAsync(
        Guid accountId,
        int limit,
        CancellationToken cancellationToken)
    {
        var pageSize = limit <= 0 ? DefaultConversationPageSize : Math.Min(limit, 100);
        var memberships = await _db.ConversationMembers
            .AsNoTracking()
            .Where(x => x.AccountId == accountId)
            .OrderBy(x => x.PinOrder.HasValue ? 0 : 1)
            .ThenBy(x => x.PinOrder)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (memberships.Count == 0)
        {
            return new ListEnvelope<ConversationSummaryDto>([], null);
        }

        var conversationIds = memberships.Select(x => x.ConversationId).ToArray();
        var conversations = await _db.Conversations
            .AsNoTracking()
            .Where(x => conversationIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var allMembers = await _db.ConversationMembers
            .AsNoTracking()
            .Where(x => conversationIds.Contains(x.ConversationId))
            .Include(x => x.Account)
            .ToListAsync(cancellationToken);

        var messages = await _db.Messages
            .AsNoTracking()
            .Where(x => conversationIds.Contains(x.ConversationId))
            .OrderByDescending(x => x.ServerSequence)
            .ToListAsync(cancellationToken);

        var lastMessages = messages
            .GroupBy(x => x.ConversationId)
            .ToDictionary(x => x.Key, x => x.First());

        var summaries = memberships
            .Select(membership =>
            {
                var conversation = conversations[membership.ConversationId];
                var memberSet = allMembers.Where(x => x.ConversationId == membership.ConversationId).ToList();
                lastMessages.TryGetValue(membership.ConversationId, out var lastMessage);
                return ToConversationSummaryDto(accountId, membership, conversation, memberSet, lastMessage);
            })
            .OrderByDescending(x => x.IsPinned)
            .ThenBy(x => x.IsPinned ? 0 : 1)
            .ThenByDescending(x => x.SortKey)
            .ToList();

        return new ListEnvelope<ConversationSummaryDto>(summaries, null);
    }

    public async Task<ListEnvelope<MessageItemDto>> ListMessagesAsync(
        Guid accountId,
        Guid conversationId,
        long? beforeSequence,
        int limit,
        CancellationToken cancellationToken)
    {
        await EnsureConversationMemberAsync(accountId, conversationId, cancellationToken);

        var pageSize = limit <= 0 ? DefaultMessagePageSize : Math.Min(limit, 100);
        var query = _db.Messages
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationId);

        if (beforeSequence.HasValue)
        {
            query = query.Where(x => x.ServerSequence < beforeSequence.Value);
        }

        var items = await query
            .OrderByDescending(x => x.ServerSequence)
            .Take(pageSize)
            .Include(x => x.SenderAccount)
            .ToListAsync(cancellationToken);

        items.Reverse();

        var nextCursor = items.Count == pageSize ? items[0].ServerSequence.ToString() : null;
        var dtos = items.Select(x => ToMessageItemDto(x, accountId)).ToList();
        return new ListEnvelope<MessageItemDto>(dtos, nextCursor);
    }

    public async Task<MessageItemDto> PostTextMessageAsync(
        Guid accountId,
        Guid conversationId,
        PostTextMessageRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedBody = NormalizeMessageBody(request.Body);
        var membership = await EnsureConversationMemberAsync(accountId, conversationId, cancellationToken);
        var conversation = await _db.Conversations
            .FirstOrDefaultAsync(x => x.Id == conversationId, cancellationToken)
            ?? throw NotFound("conversation_not_found", "대화방을 찾을 수 없습니다.");

        var existing = await _db.Messages
            .AsNoTracking()
            .Include(x => x.SenderAccount)
            .FirstOrDefaultAsync(
                x => x.ConversationId == conversationId && x.ClientRequestId == request.ClientRequestId,
                cancellationToken);

        if (existing is not null)
        {
            return ToMessageItemDto(existing, accountId);
        }

        var now = _clock.UtcNow;
        conversation.LastMessageSequence += 1;
        membership.LastReadSequence = conversation.LastMessageSequence;

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderAccountId = accountId,
            ClientRequestId = request.ClientRequestId,
            ServerSequence = conversation.LastMessageSequence,
            MessageType = MessageType.Text,
            BodyText = normalizedBody,
            CreatedAt = now
        };

        _db.Messages.Add(message);
        await _db.SaveChangesAsync(cancellationToken);

        message = await _db.Messages
            .AsNoTracking()
            .Include(x => x.SenderAccount)
            .FirstAsync(x => x.Id == message.Id, cancellationToken);

        var memberIds = await _db.ConversationMembers
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationId)
            .Select(x => x.AccountId)
            .ToListAsync(cancellationToken);

        var dto = ToMessageItemDto(message, accountId);
        await _realtimeNotifier.PublishToAccountsAsync(memberIds, "message.created", dto, cancellationToken);
        return dto;
    }

    public async Task<ReadCursorUpdatedDto> UpdateReadCursorAsync(
        Guid accountId,
        Guid conversationId,
        UpdateReadCursorRequest request,
        CancellationToken cancellationToken)
    {
        var membership = await EnsureConversationMemberAsync(accountId, conversationId, cancellationToken);
        var conversation = await _db.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == conversationId, cancellationToken)
            ?? throw NotFound("conversation_not_found", "대화방을 찾을 수 없습니다.");

        membership.LastReadSequence = Math.Max(membership.LastReadSequence, Math.Min(request.LastReadSequence, conversation.LastMessageSequence));
        await _db.SaveChangesAsync(cancellationToken);

        var payload = new ReadCursorUpdatedDto(
            conversationId.ToString(),
            accountId.ToString(),
            membership.LastReadSequence,
            _clock.UtcNow);

        var memberIds = await _db.ConversationMembers
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationId)
            .Select(x => x.AccountId)
            .ToListAsync(cancellationToken);

        await _realtimeNotifier.PublishToAccountsAsync(memberIds, "read_cursor.updated", payload, cancellationToken);
        return payload;
    }

    private async Task<ConversationMember> EnsureConversationMemberAsync(
        Guid accountId,
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var membership = await _db.ConversationMembers
            .FirstOrDefaultAsync(x => x.AccountId == accountId && x.ConversationId == conversationId, cancellationToken);

        if (membership is null)
        {
            throw NotFound("conversation_not_found", "대화방을 찾을 수 없습니다.");
        }

        return membership;
    }

    private static ConversationSummaryDto ToConversationSummaryDto(
        Guid viewerAccountId,
        ConversationMember membership,
        Conversation conversation,
        IReadOnlyCollection<ConversationMember> members,
        Message? lastMessage)
    {
        var otherMember = members.FirstOrDefault(x => x.AccountId != viewerAccountId)?.Account;
        var title = conversation.Type switch
        {
            ConversationType.Self => "나에게 메시지",
            ConversationType.Direct => otherMember?.DisplayName ?? "새 대화",
            _ => "새 그룹"
        };

        var subtitle = conversation.Type switch
        {
            ConversationType.Self => "메모와 파일을 나에게 보관해 보세요.",
            ConversationType.Direct => otherMember?.StatusMessage ?? "대화를 시작해 보세요.",
            _ => "대화를 시작해 보세요."
        };

        MessagePreviewDto? lastMessageDto = null;
        if (lastMessage is not null)
        {
            var senderId = lastMessage.SenderAccountId;
            lastMessageDto = new MessagePreviewDto(
                lastMessage.Id.ToString(),
                lastMessage.BodyText,
                lastMessage.CreatedAt,
                senderId.ToString());
        }

        var unreadCount = Math.Max(0, (int)Math.Min(int.MaxValue, conversation.LastMessageSequence - membership.LastReadSequence));

        return new ConversationSummaryDto(
            conversation.Id.ToString(),
            conversation.Type.ToString().ToLowerInvariant(),
            title,
            null,
            subtitle,
            members.Count,
            membership.IsMuted,
            membership.PinOrder.HasValue,
            lastMessage?.CreatedAt ?? conversation.CreatedAt,
            unreadCount,
            membership.LastReadSequence,
            lastMessageDto);
    }

    private static MessageItemDto ToMessageItemDto(Message message, Guid viewerAccountId)
    {
        var sender = message.SenderAccount ?? throw new InvalidOperationException("SenderAccount must be loaded.");
        return new MessageItemDto(
            message.Id.ToString(),
            message.ConversationId.ToString(),
            message.ClientRequestId,
            message.MessageType.ToString().ToLowerInvariant(),
            message.BodyText,
            message.CreatedAt,
            message.EditedAt,
            new MessageSenderDto(sender.Id.ToString(), sender.DisplayName, null),
            sender.Id == viewerAccountId,
            message.ServerSequence);
    }

    private static MeDto ToMeDto(Account account) =>
        new(account.Id.ToString(), account.DisplayName, null, account.StatusMessage);

    private static string NormalizeDisplayName(string value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new AppException(
                "display_name_required",
                "이름을 입력해 주세요.",
                fieldErrors: new Dictionary<string, string> { ["displayName"] = "이름을 입력해 주세요." });
        }

        if (normalized.Length > 40)
        {
            throw new AppException(
                "display_name_too_long",
                "이름은 40자 이하로 입력해 주세요.",
                fieldErrors: new Dictionary<string, string> { ["displayName"] = "이름은 40자 이하로 입력해 주세요." });
        }

        return normalized;
    }

    private static string NormalizeInviteCode(string value)
    {
        var normalized = value?.Trim().ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw InvalidInviteException();
        }

        return normalized;
    }

    private static string NormalizeInstallId(string value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return Guid.NewGuid().ToString();
        }

        return normalized;
    }

    private static string NormalizeMessageBody(string value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new AppException(
                "message_body_required",
                "메시지 내용을 입력해 주세요.",
                fieldErrors: new Dictionary<string, string> { ["body"] = "메시지 내용을 입력해 주세요." });
        }

        if (normalized.Length > 4000)
        {
            throw new AppException(
                "message_body_too_long",
                "메시지는 4000자 이하로 입력해 주세요.",
                fieldErrors: new Dictionary<string, string> { ["body"] = "메시지는 4000자 이하로 입력해 주세요." });
        }

        return normalized;
    }

    private static string HashInviteCode(string inviteCode)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(inviteCode));
        return Convert.ToHexString(bytes);
    }

    private static AppException InvalidInviteException() =>
        new(
            "invite_invalid",
            "초대코드가 유효하지 않습니다.",
            fieldErrors: new Dictionary<string, string> { ["inviteCode"] = "초대코드를 다시 확인해 주세요." });

    private static AppException NotFound(string code, string message) =>
        new(code, message, System.Net.HttpStatusCode.NotFound);
}
