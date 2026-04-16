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

            _ = viewModel.InitializeAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
