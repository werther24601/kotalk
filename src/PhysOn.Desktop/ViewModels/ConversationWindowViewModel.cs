using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhysOn.Contracts.Conversations;
using PhysOn.Contracts.Realtime;
using PhysOn.Desktop.Services;

namespace PhysOn.Desktop.ViewModels;

public partial class ConversationWindowViewModel : ViewModelBase, IAsyncDisposable
{
    private readonly PhysOnApiClient _apiClient = new();
    private readonly PhysOnRealtimeClient _realtimeClient = new();
    private readonly ConversationWindowLaunch _launchContext;

    public ConversationWindowViewModel(ConversationWindowLaunch launchContext)
    {
        _launchContext = launchContext;
        ConversationTitle = launchContext.ConversationTitle;
        ConversationSubtitle = launchContext.ConversationSubtitle;
        SendMessageCommand = new AsyncRelayCommand(SendMessageAsync, CanSendMessage);
        ReloadCommand = new AsyncRelayCommand(LoadMessagesAsync, () => !IsBusy);

        _realtimeClient.ConnectionStateChanged += HandleRealtimeConnectionStateChanged;
        _realtimeClient.MessageCreated += HandleMessageCreated;
    }

    public ObservableCollection<MessageRowViewModel> Messages { get; } = [];

    public IAsyncRelayCommand SendMessageCommand { get; }
    public IAsyncRelayCommand ReloadCommand { get; }

    [ObservableProperty] private string conversationTitle = string.Empty;
    [ObservableProperty] private string conversationSubtitle = string.Empty;
    [ObservableProperty] private string composerText = string.Empty;
    [ObservableProperty] private string statusText = "·";
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? errorText;

    public string ConversationGlyph =>
        string.IsNullOrWhiteSpace(ConversationTitle) ? "PO" : ConversationTitle.Trim()[..Math.Min(2, ConversationTitle.Trim().Length)];

    public bool HasErrorText => !string.IsNullOrWhiteSpace(ErrorText);

    public async Task InitializeAsync()
    {
        await LoadMessagesAsync();

        try
        {
            var bootstrap = await _apiClient.GetBootstrapAsync(
                _launchContext.ApiBaseUrl,
                _launchContext.AccessToken,
                CancellationToken.None);
            await _realtimeClient.ConnectAsync(bootstrap.Ws.Url, _launchContext.AccessToken, CancellationToken.None);
        }
        catch (Exception exception)
        {
            ErrorText = exception.Message;
        }
    }

    public async Task SendMessageFromShortcutAsync()
    {
        if (SendMessageCommand.CanExecute(null))
        {
            await SendMessageCommand.ExecuteAsync(null);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _realtimeClient.DisposeAsync();
    }

    partial void OnComposerTextChanged(string value) => SendMessageCommand.NotifyCanExecuteChanged();
    partial void OnErrorTextChanged(string? value) => OnPropertyChanged(nameof(HasErrorText));
    partial void OnConversationTitleChanged(string value) => OnPropertyChanged(nameof(ConversationGlyph));

    private async Task LoadMessagesAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ErrorText = null;
            StatusText = "◌";

            var items = await _apiClient.GetMessagesAsync(
                _launchContext.ApiBaseUrl,
                _launchContext.AccessToken,
                _launchContext.ConversationId,
                CancellationToken.None);

            Messages.Clear();
            foreach (var item in items.Items.OrderBy(message => message.ServerSequence))
            {
                Messages.Add(MapMessage(item));
            }

            StatusText = "●";
        }
        catch (Exception exception)
        {
            ErrorText = exception.Message;
            StatusText = "×";
        }
        finally
        {
            IsBusy = false;
            ReloadCommand.NotifyCanExecuteChanged();
            SendMessageCommand.NotifyCanExecuteChanged();
        }
    }

    private async Task SendMessageAsync()
    {
        if (!CanSendMessage())
        {
            return;
        }

        var draft = ComposerText.Trim();
        var clientMessageId = Guid.NewGuid();
        ComposerText = string.Empty;

        var pendingMessage = new MessageRowViewModel
        {
            MessageId = $"pending-{Guid.NewGuid():N}",
            ClientMessageId = clientMessageId,
            Text = draft,
            SenderName = _launchContext.DisplayName,
            MetaText = "보내는 중",
            IsMine = true,
            IsPending = true,
            ServerSequence = Messages.Count == 0 ? 1 : Messages[^1].ServerSequence + 1
        };

        Messages.Add(pendingMessage);

        try
        {
            var committed = await _apiClient.SendTextMessageAsync(
                _launchContext.ApiBaseUrl,
                _launchContext.AccessToken,
                _launchContext.ConversationId,
                new PostTextMessageRequest(clientMessageId, draft),
                CancellationToken.None);

            Messages.Remove(pendingMessage);
            UpsertMessage(MapMessage(committed));
            StatusText = "●";
        }
        catch (Exception exception)
        {
            pendingMessage.IsPending = false;
            pendingMessage.IsFailed = true;
            pendingMessage.MetaText = "전송 실패";
            ErrorText = exception.Message;
        }
    }

    private bool CanSendMessage() => !IsBusy && !string.IsNullOrWhiteSpace(ComposerText);

    private void HandleRealtimeConnectionStateChanged(RealtimeConnectionState state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            StatusText = state switch
            {
                RealtimeConnectionState.Connected => "●",
                RealtimeConnectionState.Reconnecting => "◔",
                RealtimeConnectionState.Disconnected => "○",
                RealtimeConnectionState.Connecting => "◌",
                _ => StatusText
            };
        });
    }

    private void HandleMessageCreated(MessageItemDto payload)
    {
        if (!string.Equals(payload.ConversationId, _launchContext.ConversationId, StringComparison.Ordinal))
        {
            return;
        }

        Dispatcher.UIThread.Post(() => UpsertMessage(MapMessage(payload)));
    }

    private static MessageRowViewModel MapMessage(MessageItemDto item)
    {
        return new MessageRowViewModel
        {
            MessageId = item.MessageId,
            ClientMessageId = item.ClientMessageId,
            Text = item.Text,
            SenderName = item.Sender.DisplayName,
            MetaText = item.CreatedAt.LocalDateTime.ToString("HH:mm"),
            IsMine = item.IsMine,
            ServerSequence = item.ServerSequence
        };
    }

    private void UpsertMessage(MessageRowViewModel next)
    {
        var existing = Messages.FirstOrDefault(item =>
            string.Equals(item.MessageId, next.MessageId, StringComparison.Ordinal) ||
            (next.ClientMessageId != Guid.Empty && item.ClientMessageId == next.ClientMessageId));

        if (existing is not null)
        {
            var index = Messages.IndexOf(existing);
            Messages[index] = next;
            return;
        }

        Messages.Add(next);
    }
}
