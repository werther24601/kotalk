namespace PhysOn.Contracts.Common;

public sealed record MeDto(
    string UserId,
    string DisplayName,
    string? ProfileImageUrl,
    string? StatusMessage);

public sealed record SessionDto(
    string SessionId,
    string DeviceId,
    string DeviceName,
    DateTimeOffset CreatedAt);
