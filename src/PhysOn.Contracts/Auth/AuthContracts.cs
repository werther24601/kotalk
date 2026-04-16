using PhysOn.Contracts.Common;
using PhysOn.Contracts.Conversations;

namespace PhysOn.Contracts.Auth;

public sealed record DeviceRegistrationDto(
    string InstallId,
    string Platform,
    string DeviceName,
    string AppVersion);

public sealed record RegisterAlphaQuickRequest(
    string DisplayName,
    string InviteCode,
    DeviceRegistrationDto Device);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record TokenPairDto(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);

public sealed record BootstrapWsDto(
    string Url,
    string Ticket,
    DateTimeOffset TicketExpiresAt);

public sealed record BootstrapResponse(
    MeDto Me,
    SessionDto Session,
    BootstrapWsDto Ws,
    ListEnvelope<ConversationSummaryDto> Conversations);

public sealed record RegisterAlphaQuickResponse(
    MeDto Account,
    SessionDto Session,
    TokenPairDto Tokens,
    BootstrapResponse Bootstrap);

public sealed record RefreshTokenResponse(TokenPairDto Tokens);
