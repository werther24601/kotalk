using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using PhysOn.Desktop.ViewModels;

namespace PhysOn.Desktop.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _boundViewModel;

    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private async void ComposerTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                await viewModel.SendMessageFromShortcutAsync();
                e.Handled = true;
            }
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.O &&
            e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
            e.KeyModifiers.HasFlag(KeyModifiers.Shift) &&
            DataContext is MainWindowViewModel viewModel)
        {
            _ = viewModel.OpenDetachedConversationFromShortcutAsync();
            e.Handled = true;
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_boundViewModel is not null)
        {
            _boundViewModel.Messages.CollectionChanged -= Messages_OnCollectionChanged;
        }

        _boundViewModel = DataContext as MainWindowViewModel;

        if (_boundViewModel is not null)
        {
            _boundViewModel.Messages.CollectionChanged += Messages_OnCollectionChanged;
        }
    }

    private void Messages_OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is NotifyCollectionChangedAction.Add or NotifyCollectionChangedAction.Reset or NotifyCollectionChangedAction.Replace)
        {
            Dispatcher.UIThread.Post(ScrollMessagesToEnd, DispatcherPriority.Background);
        }
    }

    private void ConversationPaneHost_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel && sender is Control control && control.IsVisible)
        {
            viewModel.UpdateConversationPaneWidth(control.Bounds.Width);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_boundViewModel is not null)
        {
            _boundViewModel.Messages.CollectionChanged -= Messages_OnCollectionChanged;
            _ = _boundViewModel.DisposeAsync();
        }

        base.OnClosed(e);
    }

    private void ScrollMessagesToEnd()
    {
        if (this.FindControl<ScrollViewer>("MessagesScrollViewer") is { } scrollViewer)
        {
            scrollViewer.Offset = new Vector(scrollViewer.Offset.X, scrollViewer.Extent.Height);
        }
    }
}
