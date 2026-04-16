using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using PhysOn.Desktop.ViewModels;

namespace PhysOn.Desktop.Views;

public partial class ConversationWindow : Window
{
    private ConversationWindowViewModel? _boundViewModel;

    public ConversationWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private async void ComposerTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            if (DataContext is ConversationWindowViewModel viewModel)
            {
                await viewModel.SendMessageFromShortcutAsync();
                e.Handled = true;
            }
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_boundViewModel is not null)
        {
            _boundViewModel.Messages.CollectionChanged -= Messages_OnCollectionChanged;
        }

        _boundViewModel = DataContext as ConversationWindowViewModel;

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

    protected override void OnClosed(EventArgs e)
    {
        if (_boundViewModel is not null)
        {
            _boundViewModel.Messages.CollectionChanged -= Messages_OnCollectionChanged;
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
