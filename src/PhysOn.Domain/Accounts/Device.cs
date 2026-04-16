namespace PhysOn.Domain.Accounts;

public sealed class Device
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Account? Account { get; set; }
    public string InstallId { get; set; } = string.Empty;
    public string Platform { get; set; } = "windows";
    public string DeviceName { get; set; } = "Windows PC";
    public string AppVersion { get; set; } = "0.1.0";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastSeenAt { get; set; }
}
