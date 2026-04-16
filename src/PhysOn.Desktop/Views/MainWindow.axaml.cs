using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using PhysOn.Desktop.ViewModels;

namespace PhysOn.Desktop.Views;

public partial class MainWindow : Window
{
    private bool _initialLayoutApplied;
    private MainWindowViewModel? _boundViewModel;

    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Opened += OnOpened;
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

        if (e.Key == Key.K &&
            e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
            DataContext is MainWindowViewModel)
        {
            FocusConversationSearch(selectAll: true);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.O &&
            e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
            e.KeyModifiers.HasFlag(KeyModifiers.Shift) &&
            DataContext is MainWindowViewModel viewModel)
        {
            _ = viewModel.OpenDetachedConversationFromShortcutAsync();
            e.Handled = true;
        }
    }

    private async void ConversationSearchTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        if (e.Key == Key.Enter)
        {
            await viewModel.ActivateSearchResultAsync(detach: e.KeyModifiers.HasFlag(KeyModifiers.Control));
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape)
        {
            viewModel.ClearSearch();
            e.Handled = true;
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_boundViewModel is not null)
        {
            _boundViewModel.Messages.CollectionChanged -= Messages_OnCollectionChanged;
            _boundViewModel.PropertyChanged -= ViewModel_OnPropertyChanged;
        }

        _boundViewModel = DataContext as MainWindowViewModel;

        if (_boundViewModel is not null)
        {
            _boundViewModel.Messages.CollectionChanged += Messages_OnCollectionChanged;
            _boundViewModel.PropertyChanged += ViewModel_OnPropertyChanged;
        }
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        ApplySuggestedWindowLayout(force: true);
    }

    private void ViewModel_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainWindowViewModel.ShowOnboarding) or nameof(MainWindowViewModel.ShowShell))
        {
            ApplySuggestedWindowLayout(force: !_initialLayoutApplied || !(_boundViewModel?.HasPersistedWindowBounds ?? true));
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
            _boundViewModel.CaptureWindowLayout(Width, Height, WindowState == WindowState.Maximized);
            _boundViewModel.Messages.CollectionChanged -= Messages_OnCollectionChanged;
            _boundViewModel.PropertyChanged -= ViewModel_OnPropertyChanged;
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

    private void ApplySuggestedWindowLayout(bool force)
    {
        if (_boundViewModel is null)
        {
            return;
        }

        if (_initialLayoutApplied && !force)
        {
            return;
        }

        var (minWidth, minHeight) = _boundViewModel.GetSuggestedWindowConstraints();
        MinWidth = minWidth;
        MinHeight = minHeight;

        var (suggestedWidth, suggestedHeight, maximized) = _boundViewModel.GetSuggestedWindowLayout();
        Width = suggestedWidth;
        Height = suggestedHeight;
        WindowState = maximized ? WindowState.Maximized : WindowState.Normal;
        _initialLayoutApplied = true;
    }

    private void FocusConversationSearch(bool selectAll)
    {
        if (this.FindControl<TextBox>("ConversationSearchTextBox") is not { } searchBox)
        {
            return;
        }

        searchBox.Focus();
        if (selectAll)
        {
            searchBox.SelectAll();
        }
    }
}
