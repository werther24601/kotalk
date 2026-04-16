namespace PhysOn.Domain.Accounts;

public sealed class Account
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? StatusMessage { get; set; }
    public string Locale { get; set; } = "ko-KR";
    public DateTimeOffset CreatedAt { get; set; }

    public List<Device> Devices { get; } = [];
    public List<Session> Sessions { get; } = [];
}
