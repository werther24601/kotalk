namespace PhysOn.Domain.Accounts;

public sealed class Session
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Account? Account { get; set; }
    public Guid DeviceId { get; set; }
    public Device? Device { get; set; }
    public string RefreshTokenHash { get; set; } = string.Empty;
    public Guid TokenFamilyId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastSeenAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;
}
