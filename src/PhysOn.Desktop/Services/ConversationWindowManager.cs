using Avalonia.Controls;
using PhysOn.Desktop.ViewModels;
using PhysOn.Desktop.Views;

namespace PhysOn.Desktop.Services;

public sealed class ConversationWindowManager : IConversationWindowManager
{
    private readonly Dictionary<string, ConversationWindow> _openWindows = new(StringComparer.Ordinal);

    public event Action<int>? WindowCountChanged;

    public Task ShowOrFocusAsync(ConversationWindowLaunch launchContext, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_openWindows.TryGetValue(launchContext.ConversationId, out var existingWindow))
        {
            existingWindow.WindowState = WindowState.Normal;
            existingWindow.Activate();
            return Task.CompletedTask;
        }

        var viewModel = new ConversationWindowViewModel(launchContext);
        var window = new ConversationWindow
        {
            DataContext = viewModel,
            Title = launchContext.ConversationTitle
        };

        window.Closed += (_, _) => _ = HandleWindowClosedAsync(launchContext.ConversationId, viewModel);

        _openWindows[launchContext.ConversationId] = window;
        WindowCountChanged?.Invoke(_openWindows.Count);

        window.Show();
        _ = viewModel.InitializeAsync();
        return Task.CompletedTask;
    }

    private async Task HandleWindowClosedAsync(string conversationId, ConversationWindowViewModel viewModel)
    {
        _openWindows.Remove(conversationId);
        WindowCountChanged?.Invoke(_openWindows.Count);
        await viewModel.DisposeAsync();
    }
}
