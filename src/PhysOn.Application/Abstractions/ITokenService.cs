using System.Security.Claims;
using PhysOn.Domain.Accounts;

namespace PhysOn.Application.Abstractions;

public interface ITokenService
{
    IssuedTokenSet IssueTokens(Account account, Session session, Device device, DateTimeOffset now);
    IssuedRealtimeTicket IssueRealtimeTicket(Account account, Session session, Device device, DateTimeOffset now);
    ClaimsPrincipal? TryReadPrincipal(string accessToken);
    ClaimsPrincipal? TryReadRealtimePrincipal(string accessToken);
}

public sealed record IssuedTokenSet(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    string RefreshTokenHash,
    DateTimeOffset RefreshTokenExpiresAt);

public sealed record IssuedRealtimeTicket(
    string Token,
    DateTimeOffset ExpiresAt);
