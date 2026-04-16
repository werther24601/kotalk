using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PhysOn.Application.Abstractions;
using PhysOn.Infrastructure.Auth;
using PhysOn.Infrastructure.Clock;
using PhysOn.Infrastructure.Persistence;
using PhysOn.Infrastructure.Realtime;

namespace PhysOn.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPhysOnInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("Main") ?? "Data Source=vs-messenger.db";
        var jwtOptions = configuration.GetSection("Auth:Jwt").Get<JwtOptions>() ?? new JwtOptions();
        ValidateJwtOptions(jwtOptions, environment);

        services.Configure<JwtOptions>(configuration.GetSection("Auth:Jwt"));
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<WebSocketConnectionHub>();
        services.AddSingleton<IRealtimeNotifier>(sp => sp.GetRequiredService<WebSocketConnectionHub>());
        services.AddScoped<DatabaseInitializer>();

        services.AddDbContext<PhysOnDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<PhysOnDbContext>());

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>, IHostEnvironment>((options, jwtOptionsAccessor, hostEnvironment) =>
            {
                var resolvedJwtOptions = jwtOptionsAccessor.Value;
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(resolvedJwtOptions.SigningKey));

                options.RequireHttpsMetadata = !hostEnvironment.IsDevelopment() && !hostEnvironment.IsEnvironment("Testing");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = resolvedJwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = resolvedJwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var accountIdRaw = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                            ?? context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        var sessionIdRaw = context.Principal?.FindFirstValue("sid");

                        if (!Guid.TryParse(accountIdRaw, out var accountId) || !Guid.TryParse(sessionIdRaw, out var sessionId))
                        {
                            context.Fail("invalid_session_claims");
                            return;
                        }

                        var db = context.HttpContext.RequestServices.GetRequiredService<IAppDbContext>();
                        var clock = context.HttpContext.RequestServices.GetRequiredService<IClock>();
                        var session = await db.Sessions
                            .AsNoTracking()
                            .FirstOrDefaultAsync(
                                x => x.Id == sessionId && x.AccountId == accountId,
                                context.HttpContext.RequestAborted);

                        if (session is null || !session.IsActive(clock.UtcNow))
                        {
                            context.Fail("session_inactive");
                        }
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static void ValidateJwtOptions(JwtOptions jwtOptions, IHostEnvironment environment)
    {
        var signingKey = jwtOptions.SigningKey?.Trim() ?? string.Empty;
        var looksDefault =
            string.IsNullOrWhiteSpace(signingKey) ||
            signingKey.Contains("change-me", StringComparison.OrdinalIgnoreCase) ||
            signingKey.Equals("vsmessenger-dev-signing-key-change-me-2026", StringComparison.Ordinal);

        var tooShort = signingKey.Length < 32;

        if (!environment.IsDevelopment() && !environment.IsEnvironment("Testing") && (looksDefault || tooShort))
        {
            throw new InvalidOperationException(
                "운영 환경에서는 기본 JWT 서명키를 사용할 수 없습니다. Auth:Jwt:SigningKey를 32자 이상 강한 비밀값으로 설정하세요.");
        }
    }
}
