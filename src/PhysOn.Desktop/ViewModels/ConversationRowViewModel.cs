using CommunityToolkit.Mvvm.ComponentModel;

namespace PhysOn.Desktop.ViewModels;

public partial class ConversationRowViewModel : ViewModelBase
{
    [ObservableProperty] private string conversationId = string.Empty;
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string subtitle = string.Empty;
    [ObservableProperty] private string lastMessageText = string.Empty;
    [ObservableProperty] private string metaText = string.Empty;
    [ObservableProperty] private int unreadCount;
    [ObservableProperty] private bool isPinned;
    [ObservableProperty] private bool isSelected;
    [ObservableProperty] private long lastReadSequence;
    [ObservableProperty] private DateTimeOffset sortKey;

    public bool HasUnread => UnreadCount > 0;
    public string UnreadBadgeText => UnreadCount.ToString();
    public string AvatarText => string.IsNullOrWhiteSpace(Title) ? "VS" : Title.Trim()[..Math.Min(2, Title.Trim().Length)];

    partial void OnUnreadCountChanged(int value)
    {
        OnPropertyChanged(nameof(HasUnread));
        OnPropertyChanged(nameof(UnreadBadgeText));
    }

    partial void OnTitleChanged(string value)
    {
        OnPropertyChanged(nameof(AvatarText));
    }
}
