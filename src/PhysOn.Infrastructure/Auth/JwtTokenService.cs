using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PhysOn.Application.Abstractions;
using PhysOn.Domain.Accounts;

namespace PhysOn.Infrastructure.Auth;

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly TokenValidationParameters _validationParameters;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly SymmetricSecurityKey _securityKey;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _securityKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    }

    public IssuedTokenSet IssueTokens(Account account, Session session, Device device, DateTimeOffset now)
    {
        var accessTokenExpiresAt = now.AddMinutes(_options.AccessTokenMinutes);
        var refreshTokenExpiresAt = now.AddDays(_options.RefreshTokenDays);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new("sid", session.Id.ToString()),
            new("did", device.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, account.DisplayName),
            new("ver", "1")
        };

        var credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: accessTokenExpiresAt.UtcDateTime,
            signingCredentials: credentials);

        var accessToken = _tokenHandler.WriteToken(jwt);
        var refreshTokenBytes = RandomNumberGenerator.GetBytes(32);
        var refreshToken = WebEncoders.Base64UrlEncode(refreshTokenBytes);
        var refreshTokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

        return new IssuedTokenSet(
            accessToken,
            accessTokenExpiresAt,
            refreshToken,
            refreshTokenHash,
            refreshTokenExpiresAt);
    }

    public IssuedRealtimeTicket IssueRealtimeTicket(Account account, Session session, Device device, DateTimeOffset now)
    {
        var expiresAt = now.AddMinutes(Math.Max(1, _options.RealtimeTicketMinutes));
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new("sid", session.Id.ToString()),
            new("did", device.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, account.DisplayName),
            new("scp", "ws"),
            new("ver", "1")
        };

        var credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new IssuedRealtimeTicket(_tokenHandler.WriteToken(jwt), expiresAt);
    }

    public ClaimsPrincipal? TryReadPrincipal(string accessToken)
    {
        try
        {
            return _tokenHandler.ValidateToken(accessToken, _validationParameters, out _);
        }
        catch
        {
            return null;
        }
    }

    public ClaimsPrincipal? TryReadRealtimePrincipal(string accessToken)
    {
        var principal = TryReadPrincipal(accessToken);
        if (principal?.FindFirst("scp")?.Value != "ws")
        {
            return null;
        }

        return principal;
    }
}
