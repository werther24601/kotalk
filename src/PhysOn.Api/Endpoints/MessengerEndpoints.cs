using System.Net.WebSockets;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PhysOn.Api.Auth;
using PhysOn.Application.Abstractions;
using PhysOn.Application.Exceptions;
using PhysOn.Application.Services;
using PhysOn.Contracts.Auth;
using PhysOn.Contracts.Common;
using PhysOn.Contracts.Conversations;
using PhysOn.Infrastructure.Realtime;

namespace PhysOn.Api.Endpoints;

public static class MessengerEndpoints
{
    public static IEndpointRouteBuilder MapPhysOnEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/v1/auth/register/alpha-quick",
            async (
                RegisterAlphaQuickRequest request,
                MessengerApplicationService service,
                HttpContext httpContext,
                CancellationToken cancellationToken) =>
            {
                ApplyNoStoreHeaders(httpContext.Response);
                var wsUrl = BuildWsUrl(httpContext);
                var response = await service.RegisterAlphaQuickAsync(request, wsUrl, cancellationToken);
                return Results.Ok(new ApiEnvelope<RegisterAlphaQuickResponse>(response));
            })
            .RequireRateLimiting("auth");

        endpoints.MapPost(
            "/v1/auth/token/refresh",
            async (
                RefreshTokenRequest request,
                HttpContext httpContext,
                IAppDbContext db,
                IClock clock,
                ITokenService tokenService,
                CancellationToken cancellationToken) =>
            {
                ApplyNoStoreHeaders(httpContext.Response);
                var refreshToken = request.RefreshToken?.Trim();
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    throw SessionExpired();
                }

                var now = clock.UtcNow;
                var refreshTokenHash = HashRefreshToken(refreshToken);
                var session = await db.Sessions
                    .Include(x => x.Account)
                    .Include(x => x.Device)
                    .FirstOrDefaultAsync(x => x.RefreshTokenHash == refreshTokenHash, cancellationToken);

                if (session is null)
                {
                    throw SessionExpired();
                }

                if (session.RevokedAt is not null)
                {
                    throw SessionRevoked();
                }

                if (!session.IsActive(now))
                {
                    session.RevokedAt = now;
                    await db.SaveChangesAsync(cancellationToken);
                    throw SessionExpired();
                }

                if (session.Account is null || session.Device is null)
                {
                    session.RevokedAt = now;
                    await db.SaveChangesAsync(cancellationToken);
                    throw SessionRevoked();
                }

                var issuedTokens = tokenService.IssueTokens(session.Account, session, session.Device, now);
                session.RefreshTokenHash = issuedTokens.RefreshTokenHash;
                session.ExpiresAt = issuedTokens.RefreshTokenExpiresAt;
                session.LastSeenAt = now;
                session.Device.LastSeenAt = now;

                await db.SaveChangesAsync(cancellationToken);

                return Results.Ok(
                    new ApiEnvelope<RefreshTokenResponse>(
                        new RefreshTokenResponse(ToTokenPairDto(issuedTokens))));
            })
            .RequireRateLimiting("auth");

        var authorized = endpoints.MapGroup("/v1").RequireAuthorization();

        authorized.MapGet(
            "/me",
            async (
                ClaimsPrincipal user,
                HttpContext httpContext,
                MessengerApplicationService service,
                CancellationToken cancellationToken) =>
            {
                ApplyNoStoreHeaders(httpContext.Response);
                var response = await service.GetMeAsync(user.RequireAccountId(), cancellationToken);
                return Results.Ok(new ApiEnvelope<MeDto>(response));
            });

        authorized.MapGet(
            "/bootstrap",
            async (
                ClaimsPrincipal user,
                MessengerApplicationService service,
                HttpContext httpContext,
                CancellationToken cancellationToken) =>
            {
                ApplyNoStoreHeaders(httpContext.Response);
                var response = await service.GetBootstrapAsync(
                    user.RequireAccountId(),
                    user.RequireSessionId(),
                    BuildWsUrl(httpContext),
                    cancellationToken);
                return Results.Ok(new ApiEnvelope<BootstrapResponse>(response));
            });

        authorized.MapGet(
            "/conversations",
            async (
                ClaimsPrincipal user,
                HttpContext httpContext,
                MessengerApplicationService service,
                int? limit,
                CancellationToken cancellationToken) =>
            {
                ApplyNoStoreHeaders(httpContext.Response);
                var response = await service.ListConversationsAsync(user.RequireAccountId(), limit ?? 50, cancellationToken);
                return Results.Ok(new ApiEnvelope<ListEnvelope<ConversationSummaryDto>>(response));
            });

        authorized.MapGet(
            "/conversations/{conversationId:guid}/messages",
            async (
                Guid conversationId,
                long? beforeSequence,
                int? limit,
                ClaimsPrincipal user,
                HttpContext httpContext,
                MessengerApplicationService service,
                CancellationToken cancellationToken) =>
            {
                ApplyNoStoreHeaders(httpContext.Response);
                var response = await service.ListMessagesAsync(
                    user.RequireAccountId(),
                    conversationId,
                    beforeSequence,
                    limit ?? 50,
                    cancellationToken);
                return Results.Ok(new ApiEnvelope<ListEnvelope<MessageItemDto>>(response));
            });

        authorized.MapPost(
            "/conversations/{conversationId:guid}/messages",
            async (
                Guid conversationId,
                PostTextMessageRequest request,
                ClaimsPrincipal user,
                MessengerApplicationService service,
                CancellationToken cancellationToken) =>
            {
                var response = await service.PostTextMessageAsync(user.RequireAccountId(), conversationId, request, cancellationToken);
                return Results.Ok(new ApiEnvelope<MessageItemDto>(response));
            });

        authorized.MapPost(
            "/conversations/{conversationId:guid}/read-cursor",
            async (
                Guid conversationId,
                UpdateReadCursorRequest request,
                ClaimsPrincipal user,
                MessengerApplicationService service,
                CancellationToken cancellationToken) =>
            {
                var response = await service.UpdateReadCursorAsync(user.RequireAccountId(), conversationId, request, cancellationToken);
                return Results.Ok(new ApiEnvelope<ReadCursorUpdatedDto>(response));
            });

        endpoints.MapGet(
            "/v1/realtime/ws",
            async (
                HttpContext httpContext,
                IAppDbContext db,
                IClock clock,
                ITokenService tokenService,
                WebSocketConnectionHub connectionHub,
                CancellationToken cancellationToken) =>
            {
                if (!httpContext.WebSockets.IsWebSocketRequest)
                {
                    return Results.BadRequest(new ApiErrorEnvelope(new ApiError("websocket_required", "WebSocket 연결이 필요합니다.")));
                }

                var (bearerToken, fromQueryString) = ReadBearerToken(httpContext);
                var principal = fromQueryString
                    ? tokenService.TryReadRealtimePrincipal(bearerToken)
                    : tokenService.TryReadPrincipal(bearerToken);
                if (principal is null)
                {
                    return Results.Unauthorized();
                }

                var accountId = principal.RequireAccountId();
                var sessionId = principal.RequireSessionId();
                var session = await db.Sessions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == sessionId && x.AccountId == accountId,
                        cancellationToken);

                if (session is null || !session.IsActive(clock.UtcNow))
                {
                    return Results.Unauthorized();
                }

                using var socket = await httpContext.WebSockets.AcceptWebSocketAsync();
                await connectionHub.AcceptConnectionAsync(accountId, sessionId, socket, cancellationToken);
                return Results.Empty;
            })
            .RequireRateLimiting("realtime");

        return endpoints;
    }

    private static (string Token, bool FromQueryString) ReadBearerToken(HttpContext httpContext)
    {
        const string prefix = "Bearer ";
        var authorizationHeader = httpContext.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authorizationHeader) &&
            authorizationHeader.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return (authorizationHeader[prefix.Length..].Trim(), false);
        }

        var queryToken = httpContext.Request.Query["access_token"].ToString();
        if (!string.IsNullOrWhiteSpace(queryToken))
        {
            return (queryToken.Trim(), true);
        }

        throw new AppException(
            "unauthorized",
            "인증 토큰이 필요합니다.",
            System.Net.HttpStatusCode.Unauthorized);
    }

    private static string BuildWsUrl(HttpContext httpContext)
    {
        var configuration = httpContext.RequestServices.GetRequiredService<IConfiguration>();
        var configuredOrigin = configuration["ClientFacing:PublicOrigin"]?.Trim();

        if (Uri.TryCreate(configuredOrigin, UriKind.Absolute, out var publicOrigin))
        {
            var publicScheme = string.Equals(publicOrigin.Scheme, "https", StringComparison.OrdinalIgnoreCase) ? "wss" : "ws";
            return $"{publicScheme}://{publicOrigin.Authority}/v1/realtime/ws";
        }

        var scheme = string.Equals(httpContext.Request.Scheme, "https", StringComparison.OrdinalIgnoreCase) ? "wss" : "ws";
        return $"{scheme}://{httpContext.Request.Host}/v1/realtime/ws";
    }

    private static void ApplyNoStoreHeaders(HttpResponse response)
    {
        response.Headers["Cache-Control"] = "no-store, no-cache, max-age=0";
        response.Headers["Pragma"] = "no-cache";
        response.Headers["Expires"] = "0";
    }

    private static TokenPairDto ToTokenPairDto(IssuedTokenSet issuedTokens)
    {
        return new TokenPairDto(
            issuedTokens.AccessToken,
            issuedTokens.AccessTokenExpiresAt,
            issuedTokens.RefreshToken,
            issuedTokens.RefreshTokenExpiresAt);
    }

    private static string HashRefreshToken(string refreshToken)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
    }

    private static AppException SessionExpired()
    {
        return new AppException(
            "session_expired",
            "세션이 만료되었습니다. 다시 로그인해 주세요.",
            System.Net.HttpStatusCode.Unauthorized);
    }

    private static AppException SessionRevoked()
    {
        return new AppException(
            "session_revoked",
            "이 세션은 더 이상 사용할 수 없습니다. 다시 로그인해 주세요.",
            System.Net.HttpStatusCode.Unauthorized);
    }
}
