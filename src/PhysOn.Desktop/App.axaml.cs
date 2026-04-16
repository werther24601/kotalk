using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PhysOn.Desktop.Services;
using PhysOn.Desktop.ViewModels;
using PhysOn.Desktop.Views;

namespace PhysOn.Desktop;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var conversationWindowManager = new ConversationWindowManager();
            var workspaceLayoutStore = new WorkspaceLayoutStore();
            var viewModel = new MainWindowViewModel(conversationWindowManager, workspaceLayoutStore);
            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel,
            };

            _ = InitializeDesktopAsync(viewModel);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static async Task InitializeDesktopAsync(MainWindowViewModel viewModel)
    {
        await viewModel.InitializeAsync();

        if (string.Equals(
                Environment.GetEnvironmentVariable("KOTALK_DESKTOP_OPEN_SAMPLE_WINDOW"),
                "1",
                StringComparison.Ordinal))
        {
            await viewModel.OpenDetachedConversationFromShortcutAsync();
        }
    }
}
