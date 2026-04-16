namespace PhysOn.Desktop.Models;

public sealed record DesktopSession(
    string ApiBaseUrl,
    string AccessToken,
    string RefreshToken,
    string DisplayName,
    string? LastConversationId);
