using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhysOn.Contracts.Auth;
using PhysOn.Contracts.Conversations;
using PhysOn.Contracts.Realtime;
using PhysOn.Desktop.Models;
using PhysOn.Desktop.Services;

namespace PhysOn.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IAsyncDisposable
{
    private const string DefaultApiBaseUrl = "https://vstalk.phy.kr";
    private readonly PhysOnApiClient _apiClient = new();
    private readonly SessionStore _sessionStore = new();
    private readonly PhysOnRealtimeClient _realtimeClient = new();
    private readonly WorkspaceLayoutStore _workspaceLayoutStore;
    private readonly IConversationWindowManager _conversationWindowManager;

    private DesktopSession? _session;
    private string? _currentUserId;

    public MainWindowViewModel()
        : this(new ConversationWindowManager(), new WorkspaceLayoutStore())
    {
    }

    public MainWindowViewModel(
        IConversationWindowManager conversationWindowManager,
        WorkspaceLayoutStore workspaceLayoutStore)
    {
        _conversationWindowManager = conversationWindowManager;
        _workspaceLayoutStore = workspaceLayoutStore;
        Messages.CollectionChanged += HandleMessagesCollectionChanged;

        SignInCommand = new AsyncRelayCommand(SignInAsync, CanSignIn);
        SendMessageCommand = new AsyncRelayCommand(SendMessageAsync, CanSendMessage);
        SignOutCommand = new AsyncRelayCommand(SignOutAsync);
        ReloadCommand = new AsyncRelayCommand(ReloadAsync, () => IsAuthenticated && !IsBusy);
        ToggleAdvancedSettingsCommand = new RelayCommand(() => ShowAdvancedSettings = !ShowAdvancedSettings);
        ShowAllConversationsCommand = new RelayCommand(() => SelectedListFilter = "all");
        ShowUnreadConversationsCommand = new RelayCommand(() => SelectedListFilter = "unread");
        ShowPinnedConversationsCommand = new RelayCommand(() => SelectedListFilter = "pinned");
        ApplyAckDraftCommand = new RelayCommand(() => ApplyQuickDraft("확인했습니다."));
        ApplyShareDraftCommand = new RelayCommand(() => ApplyQuickDraft("공유드립니다.\n- "));
        ApplyTaskDraftCommand = new RelayCommand(() => ApplyQuickDraft("할 일\n- "));
        ToggleCompactModeCommand = new RelayCommand(() => IsCompactDensity = !IsCompactDensity);
        ToggleInspectorCommand = new RelayCommand(() => IsInspectorVisible = !IsInspectorVisible);
        ToggleConversationPaneCommand = new RelayCommand(() => IsConversationPaneCollapsed = !IsConversationPaneCollapsed);
        ResetWorkspaceCommand = new RelayCommand(ResetWorkspaceLayout);
        DetachConversationCommand = new AsyncRelayCommand(DetachConversationAsync, CanDetachConversation);
        DetachConversationRowCommand = new AsyncRelayCommand<ConversationRowViewModel?>(DetachConversationRowAsync, CanDetachConversationRow);
        SelectConversationCommand = new RelayCommand<ConversationRowViewModel?>(conversation =>
        {
            if (conversation is not null)
            {
                SelectedConversation = conversation;
            }
        });

        _realtimeClient.ConnectionStateChanged += HandleRealtimeConnectionStateChanged;
        _realtimeClient.SessionConnected += HandleSessionConnected;
        _realtimeClient.MessageCreated += HandleMessageCreated;
        _realtimeClient.ReadCursorUpdated += HandleReadCursorUpdated;
        _conversationWindowManager.WindowCountChanged += HandleDetachedWindowCountChanged;
    }

    public ObservableCollection<ConversationRowViewModel> Conversations { get; } = [];
    public ObservableCollection<ConversationRowViewModel> FilteredConversations { get; } = [];
    public ObservableCollection<MessageRowViewModel> Messages { get; } = [];

    public IAsyncRelayCommand SignInCommand { get; }
    public IAsyncRelayCommand SendMessageCommand { get; }
    public IAsyncRelayCommand SignOutCommand { get; }
    public IAsyncRelayCommand ReloadCommand { get; }
    public IAsyncRelayCommand DetachConversationCommand { get; }
    public IAsyncRelayCommand<ConversationRowViewModel?> DetachConversationRowCommand { get; }
    public IRelayCommand ToggleAdvancedSettingsCommand { get; }
    public IRelayCommand ShowAllConversationsCommand { get; }
    public IRelayCommand ShowUnreadConversationsCommand { get; }
    public IRelayCommand ShowPinnedConversationsCommand { get; }
    public IRelayCommand ApplyAckDraftCommand { get; }
    public IRelayCommand ApplyShareDraftCommand { get; }
    public IRelayCommand ApplyTaskDraftCommand { get; }
    public IRelayCommand ToggleCompactModeCommand { get; }
    public IRelayCommand ToggleInspectorCommand { get; }
    public IRelayCommand ToggleConversationPaneCommand { get; }
    public IRelayCommand ResetWorkspaceCommand { get; }
    public IRelayCommand<ConversationRowViewModel?> SelectConversationCommand { get; }

    [ObservableProperty] private string apiBaseUrl = DefaultApiBaseUrl;
    [ObservableProperty] private string displayName = string.Empty;
    [ObservableProperty] private string inviteCode = string.Empty;
    [ObservableProperty] private bool rememberSession = true;
    [ObservableProperty] private bool showAdvancedSettings;
    [ObservableProperty] private bool isAuthenticated;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string currentUserDisplayName = "KO";
    [ObservableProperty] private string statusLine = string.Empty;
    [ObservableProperty] private RealtimeConnectionState realtimeState = RealtimeConnectionState.Idle;
    [ObservableProperty] private string realtimeStatusText = "준비";
    [ObservableProperty] private string? errorText;
    [ObservableProperty] private string conversationSearchText = string.Empty;
    [ObservableProperty] private string selectedListFilter = "all";
    [ObservableProperty] private string composerText = string.Empty;
    [ObservableProperty] private string selectedConversationTitle = "KoTalk";
    [ObservableProperty] private string selectedConversationSubtitle = "준비";
    [ObservableProperty] private ConversationRowViewModel? selectedConversation;
    [ObservableProperty] private bool hasErrorText;
    [ObservableProperty] private bool hasFilteredConversations;
    [ObservableProperty] private string conversationEmptyStateText = "지금 표시할 대화가 없습니다.";
    [ObservableProperty] private bool isCompactDensity = true;
    [ObservableProperty] private bool isInspectorVisible;
    [ObservableProperty] private bool isConversationPaneCollapsed;
    [ObservableProperty] private double conversationPaneWidthValue = 348;
    [ObservableProperty] private int detachedWindowCount;

    public bool ShowOnboarding => !IsAuthenticated;
    public bool ShowShell => IsAuthenticated;
    public bool IsAllFilterSelected => string.Equals(SelectedListFilter, "all", StringComparison.Ordinal);
    public bool IsUnreadFilterSelected => string.Equals(SelectedListFilter, "unread", StringComparison.Ordinal);
    public bool IsPinnedFilterSelected => string.Equals(SelectedListFilter, "pinned", StringComparison.Ordinal);
    public int TotalConversationCount => Conversations.Count;
    public int UnreadConversationCount => Conversations.Count(item => item.UnreadCount > 0);
    public int PinnedConversationCount => Conversations.Count(item => item.IsPinned);
    public bool ShowConversationEmptyState => !HasFilteredConversations;
    public string AdvancedSettingsButtonText => ShowAdvancedSettings ? "기본" : "고급";
    public string CurrentUserMonogram =>
        string.IsNullOrWhiteSpace(CurrentUserDisplayName) ? "KO" : CurrentUserDisplayName.Trim()[..Math.Min(2, CurrentUserDisplayName.Trim().Length)];
    public string AllFilterButtonText => "◎";
    public string UnreadFilterButtonText => "●";
    public string PinnedFilterButtonText => "★";
    public string RealtimeStatusGlyph => RealtimeState switch
    {
        RealtimeConnectionState.Connected => "●",
        RealtimeConnectionState.Connecting => "◌",
        RealtimeConnectionState.Reconnecting => "◔",
        RealtimeConnectionState.Disconnected => "○",
        _ => "·"
    };
    public string CompactModeGlyph => IsCompactDensity ? "◫" : "◻";
    public string DensityGlyph => IsCompactDensity ? "▥" : "▤";
    public string InspectorGlyph => IsInspectorVisible ? "▣" : "□";
    public string InspectorActionGlyph => IsInspectorVisible ? "▣" : "□";
    public string ConversationPaneGlyph => IsConversationPaneCollapsed ? "›" : "‹";
    public string PaneActionGlyph => IsConversationPaneCollapsed ? "›" : "‹";
    public string SelectedConversationGlyph =>
        SelectedConversation is null ? "KO" : SelectedConversation.AvatarText;
    public bool HasSelectedConversation => SelectedConversation is not null;
    public bool HasSelectedConversationUnread => (SelectedConversation?.UnreadCount ?? 0) > 0;
    public string SelectedConversationUnreadBadgeText => (SelectedConversation?.UnreadCount ?? 0) > 99
        ? "99+"
        : (SelectedConversation?.UnreadCount ?? 0).ToString();
    public bool SelectedConversationIsPinned => SelectedConversation?.IsPinned ?? false;
    public string DetachedWindowBadgeText => DetachedWindowCount > 9 ? "9+" : DetachedWindowCount.ToString();
    public string DetachedWindowActionGlyph => HasDetachedWindows ? DetachedWindowBadgeText : "↗";
    public bool HasDetachedWindows => DetachedWindowCount > 0;
    public bool IsConversationPaneExpanded => !IsConversationPaneCollapsed;
    public double ConversationPaneWidth => IsConversationPaneCollapsed ? 0 : ConversationPaneWidthValue;
    public double InspectorPaneWidth => IsInspectorVisible ? (IsCompactDensity ? 92 : 108) : 0;
    public Thickness ConversationRowPadding => IsCompactDensity ? new Thickness(6, 5) : new Thickness(8, 6);
    public Thickness MessageBubblePadding => IsCompactDensity ? new Thickness(10, 7) : new Thickness(12, 9);
    public double ConversationAvatarSize => IsCompactDensity ? 28 : 32;
    public double ComposerMinHeight => IsCompactDensity ? 48 : 58;
    public string ComposerCounterText => $"{ComposerText.Trim().Length}";
    public string SearchWatermark => "검색";
    public string InspectorStatusText => HasDetachedWindows
        ? $"{RealtimeStatusGlyph} {DetachedWindowBadgeText}"
        : RealtimeStatusGlyph;
    public string WorkspaceModeText => HasDetachedWindows ? $"분리 창 {DetachedWindowBadgeText}" : "단일 창";
    public string StatusSummaryText => string.IsNullOrWhiteSpace(StatusLine) ? RealtimeStatusText : StatusLine;
    public string ComposerPlaceholderText => HasSelectedConversation ? "메시지" : "대화 선택";
    public string ComposerActionText => Messages.Count == 0 ? "시작" : "보내기";
    public bool ShowMessageEmptyState => Messages.Count == 0;
    public string MessageEmptyStateTitle => HasSelectedConversation ? "첫 메시지" : "대화 선택";
    public string MessageEmptyStateText => HasSelectedConversation
        ? "짧게 남기세요."
        : "목록에서 선택";

    public async Task InitializeAsync()
    {
        if (string.Equals(Environment.GetEnvironmentVariable("KOTALK_DESKTOP_SAMPLE_MODE"), "1", StringComparison.Ordinal))
        {
            LoadSampleWorkspace();
            return;
        }

        var workspaceLayout = await _workspaceLayoutStore.LoadAsync();
        if (workspaceLayout is not null)
        {
            ApplyWorkspaceLayout(workspaceLayout);
        }

        var storedSession = await _sessionStore.LoadAsync();
        if (storedSession is null)
        {
            return;
        }

        ApiBaseUrl = storedSession.ApiBaseUrl;
        _session = storedSession;
        await RestoreSessionAsync(storedSession);
    }

    public async Task SendMessageFromShortcutAsync()
    {
        if (SendMessageCommand.CanExecute(null))
        {
            await SendMessageCommand.ExecuteAsync(null);
        }
    }

    public async Task OpenDetachedConversationFromShortcutAsync()
    {
        if (DetachConversationCommand.CanExecute(null))
        {
            await DetachConversationCommand.ExecuteAsync(null);
        }
    }

    partial void OnIsAuthenticatedChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowOnboarding));
        OnPropertyChanged(nameof(ShowShell));
        ReloadCommand.NotifyCanExecuteChanged();
        SendMessageCommand.NotifyCanExecuteChanged();
        DetachConversationCommand.NotifyCanExecuteChanged();
        DetachConversationRowCommand.NotifyCanExecuteChanged();
    }

    partial void OnApiBaseUrlChanged(string value) => SignInCommand.NotifyCanExecuteChanged();
    partial void OnDisplayNameChanged(string value) => SignInCommand.NotifyCanExecuteChanged();
    partial void OnInviteCodeChanged(string value) => SignInCommand.NotifyCanExecuteChanged();
    partial void OnShowAdvancedSettingsChanged(bool value) => OnPropertyChanged(nameof(AdvancedSettingsButtonText));
    partial void OnConversationSearchTextChanged(string value) => RefreshConversationFilter();
    partial void OnSelectedListFilterChanged(string value)
    {
        OnPropertyChanged(nameof(IsAllFilterSelected));
        OnPropertyChanged(nameof(IsUnreadFilterSelected));
        OnPropertyChanged(nameof(IsPinnedFilterSelected));
        RefreshConversationFilter();
    }
    partial void OnErrorTextChanged(string? value) => HasErrorText = !string.IsNullOrWhiteSpace(value);
    partial void OnHasFilteredConversationsChanged(bool value) => OnPropertyChanged(nameof(ShowConversationEmptyState));
    partial void OnStatusLineChanged(string value) => OnPropertyChanged(nameof(StatusSummaryText));
    partial void OnComposerTextChanged(string value)
    {
        SendMessageCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(ComposerCounterText));
    }
    partial void OnRealtimeStatusTextChanged(string value) => OnPropertyChanged(nameof(StatusSummaryText));
    partial void OnRealtimeStateChanged(RealtimeConnectionState value)
    {
        OnPropertyChanged(nameof(RealtimeStatusGlyph));
        OnPropertyChanged(nameof(InspectorStatusText));
    }
    partial void OnIsCompactDensityChanged(bool value)
    {
        OnPropertyChanged(nameof(CompactModeGlyph));
        OnPropertyChanged(nameof(DensityGlyph));
        OnPropertyChanged(nameof(ConversationPaneWidth));
        OnPropertyChanged(nameof(InspectorPaneWidth));
        OnPropertyChanged(nameof(ConversationRowPadding));
        OnPropertyChanged(nameof(MessageBubblePadding));
        OnPropertyChanged(nameof(ConversationAvatarSize));
        OnPropertyChanged(nameof(ComposerMinHeight));
        _ = PersistWorkspaceLayoutAsync();
    }
    partial void OnIsInspectorVisibleChanged(bool value)
    {
        OnPropertyChanged(nameof(InspectorGlyph));
        OnPropertyChanged(nameof(InspectorActionGlyph));
        OnPropertyChanged(nameof(InspectorPaneWidth));
        _ = PersistWorkspaceLayoutAsync();
    }
    partial void OnConversationPaneWidthValueChanged(double value)
    {
        OnPropertyChanged(nameof(ConversationPaneWidth));
        _ = PersistWorkspaceLayoutAsync();
    }
    partial void OnIsConversationPaneCollapsedChanged(bool value)
    {
        OnPropertyChanged(nameof(ConversationPaneGlyph));
        OnPropertyChanged(nameof(PaneActionGlyph));
        OnPropertyChanged(nameof(IsConversationPaneExpanded));
        OnPropertyChanged(nameof(ConversationPaneWidth));
        OnPropertyChanged(nameof(SearchWatermark));
        _ = PersistWorkspaceLayoutAsync();
    }
    partial void OnDetachedWindowCountChanged(int value)
    {
        OnPropertyChanged(nameof(DetachedWindowBadgeText));
        OnPropertyChanged(nameof(DetachedWindowActionGlyph));
        OnPropertyChanged(nameof(HasDetachedWindows));
        OnPropertyChanged(nameof(InspectorStatusText));
        OnPropertyChanged(nameof(WorkspaceModeText));
    }

    partial void OnSelectedConversationChanged(ConversationRowViewModel? value)
    {
        UpdateSelectedConversationState(value?.ConversationId);
        SelectedConversationTitle = value?.Title ?? "KoTalk";
        SelectedConversationSubtitle = value?.Subtitle ?? "대화";
        OnPropertyChanged(nameof(SelectedConversationGlyph));
        OnPropertyChanged(nameof(HasSelectedConversation));
        OnPropertyChanged(nameof(HasSelectedConversationUnread));
        OnPropertyChanged(nameof(SelectedConversationUnreadBadgeText));
        OnPropertyChanged(nameof(SelectedConversationIsPinned));
        OnPropertyChanged(nameof(ComposerPlaceholderText));
        NotifyMessageStateChanged();
        SendMessageCommand.NotifyCanExecuteChanged();
        DetachConversationCommand.NotifyCanExecuteChanged();
        _ = HandleSelectedConversationChangedAsync(value);
    }

    private async Task SignInAsync()
    {
        await RunBusyAsync(async () =>
        {
            var apiBaseUrl = ResolveApiBaseUrl();
            var request = new RegisterAlphaQuickRequest(
                DisplayName.Trim(),
                InviteCode.Trim(),
                new DeviceRegistrationDto(
                    $"desktop-{Environment.MachineName.ToLowerInvariant()}",
                    "windows",
                    Environment.MachineName,
                    "0.1.0-alpha.6"));

            var response = await _apiClient.RegisterAlphaQuickAsync(apiBaseUrl, request, CancellationToken.None);
            ApiBaseUrl = apiBaseUrl;
            _session = new DesktopSession(
                apiBaseUrl,
                response.Tokens.AccessToken,
                response.Tokens.RefreshToken,
                response.Account.DisplayName,
                response.Bootstrap.Conversations.Items.FirstOrDefault()?.ConversationId);

            if (RememberSession)
            {
                await _sessionStore.SaveAsync(_session);
            }

            ApplyBootstrap(response.Bootstrap, response.Account.DisplayName, _session.LastConversationId);
            await StartRealtimeAsync(response.Bootstrap.Ws.Url, _session.AccessToken);
            StatusLine = "준비";
            NotifyMessageStateChanged();
        });
    }

    private async Task ReloadAsync()
    {
        if (_session is null)
        {
            return;
        }

        await RestoreSessionAsync(_session);
    }

    private async Task SignOutAsync()
    {
        await _realtimeClient.DisconnectAsync();
        _session = null;
        _currentUserId = null;
        await _sessionStore.ClearAsync();
        Conversations.Clear();
        FilteredConversations.Clear();
        Messages.Clear();
        IsAuthenticated = false;
        CurrentUserDisplayName = "KO";
        StatusLine = string.Empty;
        RealtimeState = RealtimeConnectionState.Idle;
        RealtimeStatusText = "준비";
        ErrorText = null;
        ApiBaseUrl = DefaultApiBaseUrl;
        ConversationSearchText = string.Empty;
        SelectedListFilter = "all";
        SelectedConversation = null;
        SelectedConversationTitle = "KoTalk";
        SelectedConversationSubtitle = "준비";
        NotifyConversationMetricsChanged();
    }

    private async Task HandleSelectedConversationChangedAsync(ConversationRowViewModel? value)
    {
        if (!IsAuthenticated || value is null || _session is null)
        {
            return;
        }

        Messages.Clear();
        NotifyMessageStateChanged();

        await RunBusyAsync(async () =>
        {
            var items = await _apiClient.GetMessagesAsync(_session.ApiBaseUrl, _session.AccessToken, value.ConversationId, CancellationToken.None);

            if (!string.Equals(SelectedConversation?.ConversationId, value.ConversationId, StringComparison.Ordinal))
            {
                return;
            }

            Messages.Clear();
            foreach (var item in items.Items)
            {
                Messages.Add(MapMessage(item));
            }

            if (Messages.Count > 0)
            {
                var lastSequence = Messages[^1].ServerSequence;
                value.LastReadSequence = lastSequence;
                value.UnreadCount = 0;
                await _apiClient.UpdateReadCursorAsync(
                    _session.ApiBaseUrl,
                    _session.AccessToken,
                    value.ConversationId,
                    new UpdateReadCursorRequest(lastSequence),
                    CancellationToken.None);
            }

            NotifyConversationMetricsChanged();
            NotifyMessageStateChanged();
            RefreshConversationFilter(value.ConversationId);

            if (_session is not null)
            {
                _session = _session with { LastConversationId = value.ConversationId };
                if (RememberSession)
                {
                    await _sessionStore.SaveAsync(_session);
                }
            }
        }, clearMessages: false);
    }

    private async Task SendMessageAsync()
    {
        if (_session is null || SelectedConversation is null || string.IsNullOrWhiteSpace(ComposerText))
        {
            return;
        }

        var draft = ComposerText.Trim();
        var clientMessageId = Guid.NewGuid();
        ComposerText = string.Empty;

        var pending = new MessageRowViewModel
        {
            MessageId = $"pending-{Guid.NewGuid():N}",
            ClientMessageId = clientMessageId,
            Text = draft,
            SenderName = CurrentUserDisplayName,
            MetaText = "전송 중",
            IsMine = true,
            IsPending = true,
            ServerSequence = Messages.Count == 0 ? 1 : Messages[^1].ServerSequence + 1
        };

        Messages.Add(pending);
        NotifyMessageStateChanged();

        try
        {
            var sent = await _apiClient.SendTextMessageAsync(
                _session.ApiBaseUrl,
                _session.AccessToken,
                SelectedConversation.ConversationId,
                new PostTextMessageRequest(clientMessageId, draft),
                CancellationToken.None);

            Messages.Remove(pending);
            var committed = MapMessage(sent);
            UpsertMessage(committed);
            UpdateConversationAfterMessage(sent);
            StatusLine = "전송";
            NotifyMessageStateChanged();
        }
        catch (Exception)
        {
            pending.IsPending = false;
            pending.IsFailed = true;
            pending.MetaText = "실패";
            ErrorText = "메시지를 보내지 못했습니다.";
            NotifyMessageStateChanged();
        }
    }

    private async Task RestoreSessionAsync(DesktopSession session)
    {
        await RunBusyAsync(async () =>
        {
            var bootstrap = await _apiClient.GetBootstrapAsync(session.ApiBaseUrl, session.AccessToken, CancellationToken.None);
            _session = session;
            ApplyBootstrap(bootstrap, session.DisplayName, session.LastConversationId);
            await StartRealtimeAsync(bootstrap.Ws.Url, session.AccessToken);
            StatusLine = "복원";
            NotifyMessageStateChanged();
        });
    }

    private async Task DetachConversationAsync()
    {
        if (_session is null || SelectedConversation is null)
        {
            return;
        }

        await ShowDetachedConversationAsync(SelectedConversation);
        StatusLine = "분리";
    }

    private async Task DetachConversationRowAsync(ConversationRowViewModel? conversation)
    {
        if (conversation is null || !CanDetachConversationRow(conversation))
        {
            return;
        }

        SelectedConversation = conversation;
        await ShowDetachedConversationAsync(conversation);
        StatusLine = "분리";
    }

    private void ApplyBootstrap(BootstrapResponse bootstrap, string displayName, string? preferredConversationId)
    {
        _currentUserId = bootstrap.Me.UserId;
        CurrentUserDisplayName = displayName;
        Conversations.Clear();

        foreach (var item in bootstrap.Conversations.Items)
        {
            Conversations.Add(new ConversationRowViewModel
            {
                ConversationId = item.ConversationId,
                Title = item.Title,
                Subtitle = item.Subtitle,
                LastMessageText = item.LastMessage?.Text ?? string.Empty,
                MetaText = FormatConversationMeta(item),
                UnreadCount = item.UnreadCount,
                IsPinned = item.IsPinned,
                LastReadSequence = item.LastReadSequence,
                SortKey = item.SortKey
            });
        }

        IsAuthenticated = true;
        ErrorText = null;
        OnPropertyChanged(nameof(CurrentUserMonogram));
        NotifyConversationMetricsChanged();
        NotifyMessageStateChanged();
        RefreshConversationFilter(preferredConversationId);

        var target = FilteredConversations.FirstOrDefault(x => x.ConversationId == preferredConversationId)
            ?? FilteredConversations.FirstOrDefault()
            ?? Conversations.FirstOrDefault();

        if (target is not null)
        {
            SelectedConversation = target;
        }
    }

    private bool CanSignIn() =>
        !IsBusy &&
        !string.IsNullOrWhiteSpace(DisplayName) &&
        !string.IsNullOrWhiteSpace(InviteCode);

    private bool CanSendMessage() =>
        !IsBusy &&
        IsAuthenticated &&
        SelectedConversation is not null &&
        !string.IsNullOrWhiteSpace(ComposerText);

    private bool CanDetachConversation() =>
        !IsBusy &&
        IsAuthenticated &&
        SelectedConversation is not null;

    private bool CanDetachConversationRow(ConversationRowViewModel? conversation) =>
        !IsBusy &&
        IsAuthenticated &&
        conversation is not null;

    private string ResolveApiBaseUrl()
    {
        var trimmed = ApiBaseUrl.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? DefaultApiBaseUrl : trimmed;
    }

    private async Task ShowDetachedConversationAsync(ConversationRowViewModel conversation)
    {
        if (_session is null)
        {
            return;
        }

        await _conversationWindowManager.ShowOrFocusAsync(new ConversationWindowLaunch(
            _session.ApiBaseUrl,
            _session.AccessToken,
            CurrentUserDisplayName,
            conversation.ConversationId,
            conversation.Title,
            conversation.Subtitle));
    }

    private void RefreshConversationFilter(string? preferredConversationId = null)
    {
        var search = ConversationSearchText.Trim();
        var filtered = Conversations
            .Where(PassesSelectedFilter)
            .Where(item => string.IsNullOrWhiteSpace(search) || MatchesConversationSearch(item, search))
            .ToList();

        FilteredConversations.Clear();
        foreach (var item in filtered)
        {
            FilteredConversations.Add(item);
        }

        HasFilteredConversations = filtered.Count > 0;
        ConversationEmptyStateText = string.IsNullOrWhiteSpace(search)
            ? (SelectedListFilter switch
            {
                "unread" => "안읽음 대화가 없습니다.",
                "pinned" => "고정한 대화가 없습니다.",
                _ => "받은함이 비어 있습니다."
            })
            : "검색 결과가 없습니다.";

        var targetId = preferredConversationId ?? SelectedConversation?.ConversationId;
        var target = !string.IsNullOrWhiteSpace(targetId)
            ? FilteredConversations.FirstOrDefault(item => item.ConversationId == targetId)
            : null;

        if (target is null)
        {
            target = FilteredConversations.FirstOrDefault();
        }

        if (!ReferenceEquals(SelectedConversation, target))
        {
            SelectedConversation = target;
        }
        else
        {
            UpdateSelectedConversationState(target?.ConversationId);
            NotifyMessageStateChanged();
        }
    }

    private bool PassesSelectedFilter(ConversationRowViewModel item)
    {
        return SelectedListFilter switch
        {
            "unread" => item.UnreadCount > 0,
            "pinned" => item.IsPinned,
            _ => true
        };
    }

    private static bool MatchesConversationSearch(ConversationRowViewModel item, string search)
    {
        return item.Title.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
               item.LastMessageText.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
               item.Subtitle.Contains(search, StringComparison.CurrentCultureIgnoreCase);
    }

    private void UpdateSelectedConversationState(string? conversationId)
    {
        foreach (var item in Conversations)
        {
            item.IsSelected = !string.IsNullOrWhiteSpace(conversationId) &&
                string.Equals(item.ConversationId, conversationId, StringComparison.Ordinal);
        }
    }

    private void NotifyConversationMetricsChanged()
    {
        OnPropertyChanged(nameof(TotalConversationCount));
        OnPropertyChanged(nameof(UnreadConversationCount));
        OnPropertyChanged(nameof(PinnedConversationCount));
    }

    public void UpdateConversationPaneWidth(double width)
    {
        if (IsConversationPaneCollapsed)
        {
            return;
        }

        var clamped = Math.Clamp(Math.Round(width), 280, 480);
        if (Math.Abs(clamped - ConversationPaneWidthValue) > 1)
        {
            ConversationPaneWidthValue = clamped;
        }
    }

    private void ApplyQuickDraft(string template)
    {
        ComposerText = string.IsNullOrWhiteSpace(ComposerText)
            ? template
            : ComposerText.TrimEnd() + Environment.NewLine + template;
    }

    private void LoadSampleWorkspace()
    {
        Conversations.Clear();
        FilteredConversations.Clear();
        Messages.Clear();

        CurrentUserDisplayName = "이안";
        DisplayName = "이안";
        InviteCode = string.Empty;
        _currentUserId = "sample-user";
        _session = new DesktopSession(
            DefaultApiBaseUrl,
            "sample-access",
            "sample-refresh",
            CurrentUserDisplayName,
            "sample-ops");
        RealtimeState = RealtimeConnectionState.Connected;
        RealtimeStatusText = "연결됨";
        StatusLine = "준비";
        IsAuthenticated = true;
        IsCompactDensity = true;
        IsInspectorVisible = false;
        IsConversationPaneCollapsed = false;
        ConversationPaneWidthValue = 348;
        DetachedWindowCount = 1;
        ErrorText = null;

        var now = DateTimeOffset.Now;
        Conversations.Add(new ConversationRowViewModel
        {
            ConversationId = "sample-ops",
            Title = "제품 운영",
            Subtitle = "레이아웃 검수 메모를 확인해 주세요.",
            LastMessageText = "레이아웃 검수 메모를 확인해 주세요.",
            MetaText = FormatConversationMeta(now.AddMinutes(-5), 2),
            UnreadCount = 2,
            IsPinned = true,
            LastReadSequence = 12,
            SortKey = now.AddMinutes(-5)
        });
        Conversations.Add(new ConversationRowViewModel
        {
            ConversationId = "sample-review",
            Title = "디자인 리뷰",
            Subtitle = "오후 2시에 포인트만 다시 볼게요.",
            LastMessageText = "오후 2시에 포인트만 다시 볼게요.",
            MetaText = FormatConversationMeta(now.AddMinutes(-22), 0),
            UnreadCount = 0,
            IsPinned = false,
            LastReadSequence = 5,
            SortKey = now.AddMinutes(-22)
        });
        Conversations.Add(new ConversationRowViewModel
        {
            ConversationId = "sample-friends",
            Title = "주말 약속",
            Subtitle = "브런치 장소만 정하면 끝.",
            LastMessageText = "브런치 장소만 정하면 끝.",
            MetaText = FormatConversationMeta(now.AddMinutes(-54), 0),
            UnreadCount = 0,
            IsPinned = false,
            LastReadSequence = 3,
            SortKey = now.AddMinutes(-54)
        });
        Conversations.Add(new ConversationRowViewModel
        {
            ConversationId = "sample-team",
            Title = "운영 팀",
            Subtitle = "오후 공유본만 마지막으로 확인해 주세요.",
            LastMessageText = "오후 공유본만 마지막으로 확인해 주세요.",
            MetaText = FormatConversationMeta(now.AddHours(-2), 1),
            UnreadCount = 1,
            IsPinned = false,
            LastReadSequence = 7,
            SortKey = now.AddHours(-2)
        });
        Conversations.Add(new ConversationRowViewModel
        {
            ConversationId = "sample-files",
            Title = "자료 모음",
            Subtitle = "최신 캡처와 빌드 경로를 정리해 두었습니다.",
            LastMessageText = "최신 캡처와 빌드 경로를 정리해 두었습니다.",
            MetaText = FormatConversationMeta(now.AddHours(-5), 0),
            UnreadCount = 0,
            IsPinned = true,
            LastReadSequence = 9,
            SortKey = now.AddHours(-5)
        });

        NotifyConversationMetricsChanged();
        RefreshConversationFilter("sample-ops");

        SelectedConversation = Conversations.FirstOrDefault(item => item.ConversationId == "sample-ops");
        Messages.Clear();
        foreach (var item in new[]
                 {
                     new MessageRowViewModel
                     {
                         MessageId = "sample-msg-1",
                         SenderName = "민지",
                         Text = "회의 전에 레이아웃 이슈만 짧게 정리해 주세요.",
                         MetaText = "08:54",
                         IsMine = false,
                         ServerSequence = 13
                     },
                     new MessageRowViewModel
                     {
                         MessageId = "sample-msg-2",
                         SenderName = "이안",
                         Text = "리스트 폭을 다시 줄이고 우측 빈 패널도 없앴어요.",
                         MetaText = "08:56",
                         IsMine = true,
                         ServerSequence = 14
                     },
                     new MessageRowViewModel
                     {
                         MessageId = "sample-msg-3",
                         SenderName = "민지",
                         Text = "좋아요. 지금 화면이면 검수하기 좋겠네요.",
                         MetaText = "08:58",
                         IsMine = false,
                         ServerSequence = 15
                     },
                     new MessageRowViewModel
                     {
                         MessageId = "sample-msg-4",
                         SenderName = "이안",
                         Text = "스크린샷 기준으로 밀도도 같이 맞췄습니다.",
                         MetaText = "09:05",
                         IsMine = true,
                         ServerSequence = 16
                     },
                     new MessageRowViewModel
                     {
                         MessageId = "sample-msg-5",
                         SenderName = "민지",
                         Text = "좋아요. 확인 흐름이 더 짧아졌어요.",
                         MetaText = "09:06",
                         IsMine = false,
                         ServerSequence = 17
                     },
                     new MessageRowViewModel
                     {
                         MessageId = "sample-msg-6",
                         SenderName = "이안",
                         Text = "분리 창은 상단 액션으로 남겨 두었습니다.",
                         MetaText = "09:07",
                         IsMine = true,
                         ServerSequence = 18
                     },
                     new MessageRowViewModel
                     {
                         MessageId = "sample-msg-7",
                         SenderName = "민지",
                         Text = "이 정도면 데스크톱 검수용 화면으로 충분하겠네요.",
                         MetaText = "09:08",
                         IsMine = false,
                         ServerSequence = 19
                     },
                     new MessageRowViewModel
                     {
                         MessageId = "sample-msg-8",
                         SenderName = "이안",
                         Text = "검색과 필터는 한 줄 안에서 끝나도록 다시 정리할게요.",
                         MetaText = "09:10",
                         IsMine = true,
                         ServerSequence = 20
                     },
                     new MessageRowViewModel
                     {
                         MessageId = "sample-msg-9",
                         SenderName = "민지",
                         Text = "좋아요. 설명보다 눌리는 구조가 더 중요해요.",
                         MetaText = "09:11",
                         IsMine = false,
                         ServerSequence = 21
                     },
                     new MessageRowViewModel
                     {
                         MessageId = "sample-msg-10",
                         SenderName = "이안",
                         Text = "작성창도 짧은 액션만 남기고 텍스트는 줄였습니다.",
                         MetaText = "09:12",
                         IsMine = true,
                         ServerSequence = 22
                     },
                     new MessageRowViewModel
                     {
                         MessageId = "sample-msg-11",
                         SenderName = "민지",
                         Text = "이제 목록과 대화가 한 화면에서 훨씬 빠르게 읽히네요.",
                         MetaText = "09:13",
                         IsMine = false,
                         ServerSequence = 23
                     }
                 })
        {
            Messages.Add(item);
        }

        OnPropertyChanged(nameof(CurrentUserMonogram));
        NotifyMessageStateChanged();
    }

    private void HandleMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        NotifyMessageStateChanged();
    }

    private void NotifyMessageStateChanged()
    {
        OnPropertyChanged(nameof(ShowMessageEmptyState));
        OnPropertyChanged(nameof(MessageEmptyStateTitle));
        OnPropertyChanged(nameof(MessageEmptyStateText));
        OnPropertyChanged(nameof(ComposerPlaceholderText));
        OnPropertyChanged(nameof(ComposerActionText));
    }

    private async Task StartRealtimeAsync(string wsUrl, string accessToken)
    {
        await _realtimeClient.ConnectAsync(wsUrl, accessToken, CancellationToken.None);
    }

    private async Task RunBusyAsync(Func<Task> action, bool clearMessages = true)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ErrorText = null;
            if (clearMessages)
            {
                StatusLine = "동기화";
            }
            await action();
        }
        catch (Exception exception)
        {
            ErrorText = exception.Message;
            if (clearMessages)
            {
                Messages.Clear();
            }
        }
        finally
        {
            IsBusy = false;
            SignInCommand.NotifyCanExecuteChanged();
            SendMessageCommand.NotifyCanExecuteChanged();
            ReloadCommand.NotifyCanExecuteChanged();
            DetachConversationCommand.NotifyCanExecuteChanged();
            DetachConversationRowCommand.NotifyCanExecuteChanged();
        }
    }

    private static MessageRowViewModel MapMessage(MessageItemDto item)
    {
        return new MessageRowViewModel
        {
            MessageId = item.MessageId,
            ClientMessageId = item.ClientMessageId,
            Text = item.Text,
            SenderName = item.Sender.DisplayName,
            MetaText = $"{item.CreatedAt.LocalDateTime:HH:mm}",
            IsMine = item.IsMine,
            ServerSequence = item.ServerSequence
        };
    }

    private static string FormatConversationMeta(ConversationSummaryDto item)
    {
        return FormatConversationMeta(item.LastMessage?.CreatedAt ?? item.SortKey, item.UnreadCount);
    }

    private static string FormatConversationMeta(DateTimeOffset timestamp, int unreadCount)
    {
        var timeText = timestamp.LocalDateTime.ToString("HH:mm");
        return unreadCount > 0 ? $"{timeText} · {unreadCount}" : timeText;
    }

    private void HandleRealtimeConnectionStateChanged(RealtimeConnectionState state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            RealtimeState = state;
            RealtimeStatusText = state switch
            {
                RealtimeConnectionState.Connecting => "동기화",
                RealtimeConnectionState.Connected => "연결됨",
                RealtimeConnectionState.Reconnecting => "다시 연결",
                RealtimeConnectionState.Disconnected => "오프라인",
                _ => "준비"
            };
        });
    }

    private void HandleSessionConnected(SessionConnectedDto payload)
    {
        Dispatcher.UIThread.Post(() =>
        {
            RealtimeState = RealtimeConnectionState.Connected;
            RealtimeStatusText = "연결됨";
        });
    }

    private void HandleMessageCreated(MessageItemDto payload)
    {
        Dispatcher.UIThread.Post(() =>
        {
            UpdateConversationAfterMessage(payload);

            if (SelectedConversation?.ConversationId == payload.ConversationId)
            {
                UpsertMessage(MapMessage(payload));
            }
        });
    }

    private void HandleReadCursorUpdated(ReadCursorUpdatedDto payload)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_currentUserId is null || !string.Equals(payload.AccountId, _currentUserId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var conversation = Conversations.FirstOrDefault(item => item.ConversationId == payload.ConversationId);
            if (conversation is null)
            {
                return;
            }

            conversation.LastReadSequence = payload.LastReadSequence;
            conversation.UnreadCount = 0;
            conversation.MetaText = FormatConversationMeta(conversation.SortKey, 0);
            NotifyConversationMetricsChanged();
            RefreshConversationFilter(conversation.ConversationId);
        });
    }

    private void HandleDetachedWindowCountChanged(int count)
    {
        Dispatcher.UIThread.Post(() => DetachedWindowCount = count);
    }

    private void UpdateConversationAfterMessage(MessageItemDto payload)
    {
        var conversation = Conversations.FirstOrDefault(item => item.ConversationId == payload.ConversationId);
        if (conversation is null)
        {
            return;
        }

        conversation.LastMessageText = payload.Text;
        conversation.SortKey = payload.CreatedAt;
        conversation.LastReadSequence = payload.IsMine
            ? Math.Max(conversation.LastReadSequence, payload.ServerSequence)
            : conversation.LastReadSequence;
        conversation.UnreadCount = payload.IsMine
            ? 0
            : (SelectedConversation?.ConversationId == payload.ConversationId
                ? conversation.UnreadCount
                : Math.Max(conversation.UnreadCount + 1, 1));
        conversation.MetaText = FormatConversationMeta(conversation.SortKey, conversation.UnreadCount);
        NotifyConversationMetricsChanged();
        ReorderConversations(conversation.ConversationId);
    }

    private void UpsertMessage(MessageRowViewModel next)
    {
        var items = Messages.ToList();
        var existingIndex = items.FindIndex(item =>
            string.Equals(item.MessageId, next.MessageId, StringComparison.Ordinal) ||
            (next.ClientMessageId != Guid.Empty && item.ClientMessageId == next.ClientMessageId));

        if (existingIndex >= 0)
        {
            items[existingIndex] = next;
        }
        else
        {
            items.Add(next);
        }

        var ordered = items
            .OrderBy(item => item.ServerSequence)
            .ThenBy(item => item.IsPending ? 1 : 0)
            .ToList();

        Messages.Clear();
        foreach (var item in ordered)
        {
            Messages.Add(item);
        }
    }

    private void ReorderConversations(string? selectedConversationId)
    {
        var ordered = Conversations
            .OrderByDescending(item => item.IsPinned)
            .ThenByDescending(item => item.SortKey)
            .ToList();

        Conversations.Clear();
        foreach (var item in ordered)
        {
            Conversations.Add(item);
        }

        RefreshConversationFilter(selectedConversationId);
    }

    private void ResetWorkspaceLayout()
    {
        IsCompactDensity = true;
        IsInspectorVisible = false;
        IsConversationPaneCollapsed = false;
        ConversationPaneWidthValue = 348;
        StatusLine = "초기화";
    }

    private void ApplyWorkspaceLayout(DesktopWorkspaceLayout layout)
    {
        IsCompactDensity = layout.IsCompactDensity;
        IsInspectorVisible = layout.IsInspectorVisible;
        IsConversationPaneCollapsed = layout.IsConversationPaneCollapsed;
        ConversationPaneWidthValue = Math.Clamp(layout.ConversationPaneWidth, 280, 480);
    }

    private Task PersistWorkspaceLayoutAsync()
    {
        return _workspaceLayoutStore.SaveAsync(new DesktopWorkspaceLayout(
            IsCompactDensity,
            IsInspectorVisible,
            IsConversationPaneCollapsed,
            ConversationPaneWidthValue));
    }

    public async ValueTask DisposeAsync()
    {
        Messages.CollectionChanged -= HandleMessagesCollectionChanged;
        _conversationWindowManager.WindowCountChanged -= HandleDetachedWindowCountChanged;
        _realtimeClient.ConnectionStateChanged -= HandleRealtimeConnectionStateChanged;
        _realtimeClient.SessionConnected -= HandleSessionConnected;
        _realtimeClient.MessageCreated -= HandleMessageCreated;
        _realtimeClient.ReadCursorUpdated -= HandleReadCursorUpdated;
        await _realtimeClient.DisposeAsync();
    }
}
