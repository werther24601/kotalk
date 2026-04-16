namespace PhysOn.Desktop.Models;

public sealed record DesktopWorkspaceLayout(
    bool IsCompactDensity,
    bool IsInspectorVisible,
    bool IsConversationPaneCollapsed,
    double ConversationPaneWidth = 304,
    double? WindowWidth = null,
    double? WindowHeight = null,
    bool IsWindowMaximized = false);
