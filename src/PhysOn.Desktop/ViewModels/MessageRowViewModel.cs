using CommunityToolkit.Mvvm.ComponentModel;

namespace PhysOn.Desktop.ViewModels;

public partial class MessageRowViewModel : ViewModelBase
{
    [ObservableProperty] private string messageId = string.Empty;
    [ObservableProperty] private Guid clientMessageId;
    [ObservableProperty] private string text = string.Empty;
    [ObservableProperty] private string senderName = string.Empty;
    [ObservableProperty] private string metaText = string.Empty;
    [ObservableProperty] private bool isMine;
    [ObservableProperty] private bool isPending;
    [ObservableProperty] private bool isFailed;
    [ObservableProperty] private long serverSequence;

    public bool ShowSenderName => !IsMine;

    partial void OnIsMineChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowSenderName));
    }
}
