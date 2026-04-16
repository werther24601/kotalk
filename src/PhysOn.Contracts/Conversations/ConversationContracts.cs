namespace PhysOn.Contracts.Conversations;

public sealed record MessagePreviewDto(
    string MessageId,
    string Text,
    DateTimeOffset CreatedAt,
    string SenderUserId);

public sealed record ConversationSummaryDto(
    string ConversationId,
    string Type,
    string Title,
    string? AvatarUrl,
    string Subtitle,
    int MemberCount,
    bool IsMuted,
    bool IsPinned,
    DateTimeOffset SortKey,
    int UnreadCount,
    long LastReadSequence,
    MessagePreviewDto? LastMessage);

public sealed record MessageSenderDto(
    string UserId,
    string DisplayName,
    string? ProfileImageUrl);

public sealed record MessageItemDto(
    string MessageId,
    string ConversationId,
    Guid ClientMessageId,
    string Kind,
    string Text,
    DateTimeOffset CreatedAt,
    DateTimeOffset? EditedAt,
    MessageSenderDto Sender,
    bool IsMine,
    long ServerSequence);

public sealed record PostTextMessageRequest(
    Guid ClientRequestId,
    string Body);

public sealed record UpdateReadCursorRequest(long LastReadSequence);

public sealed record ReadCursorUpdatedDto(
    string ConversationId,
    string AccountId,
    long LastReadSequence,
    DateTimeOffset UpdatedAt);
