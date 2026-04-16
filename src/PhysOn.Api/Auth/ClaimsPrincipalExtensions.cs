using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PhysOn.Application.Exceptions;

namespace PhysOn.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    public static Guid RequireAccountId(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return ParseGuid(raw, "invalid_account_claim");
    }

    public static Guid RequireSessionId(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue("sid");
        return ParseGuid(raw, "invalid_session_claim");
    }

    public static Guid RequireDeviceId(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue("did");
        return ParseGuid(raw, "invalid_device_claim");
    }

    private static Guid ParseGuid(string? raw, string code)
    {
        if (Guid.TryParse(raw, out var value))
        {
            return value;
        }

        throw new AppException(code, "인증 정보가 올바르지 않습니다.", System.Net.HttpStatusCode.Unauthorized);
    }
}
