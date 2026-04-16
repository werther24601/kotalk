namespace PhysOn.Domain.Invites;

public sealed class Invite
{
    public Guid Id { get; set; }
    public string CodeHash { get; set; } = string.Empty;
    public Guid? IssuedByAccountId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public int MaxUses { get; set; }
    public int UsedCount { get; set; }

    public bool CanUse(DateTimeOffset now)
    {
        if (RevokedAt is not null)
        {
            return false;
        }

        if (ExpiresAt is not null && ExpiresAt <= now)
        {
            return false;
        }

        return UsedCount < MaxUses;
    }
}
