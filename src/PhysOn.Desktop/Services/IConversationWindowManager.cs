namespace PhysOn.Desktop.Services;

public interface IConversationWindowManager
{
    event Action<int>? WindowCountChanged;

    Task ShowOrFocusAsync(ConversationWindowLaunch launchContext, CancellationToken cancellationToken = default);
}

public sealed record ConversationWindowLaunch(
    string ApiBaseUrl,
    string AccessToken,
    string DisplayName,
    string ConversationId,
    string ConversationTitle,
    string ConversationSubtitle);
