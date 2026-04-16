namespace PhysOn.Infrastructure.Auth;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "PhysOn";
    public string Audience { get; set; } = "PhysOn.Desktop";
    public string SigningKey { get; set; } = "vsmessenger-dev-signing-key-change-me-2026";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 30;
    public int RealtimeTicketMinutes { get; set; } = 15;
}
