using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PhysOn.Domain.Invites;

namespace PhysOn.Infrastructure.Persistence;

public sealed class DatabaseInitializer
{
    private readonly PhysOnDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        PhysOnDbContext dbContext,
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<DatabaseInitializer> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await _dbContext.Invites.AnyAsync(cancellationToken))
        {
            return;
        }

        var configuredInviteCodes = _configuration.GetSection("Bootstrap:InviteCodes").Get<string[]>() ?? [];
        var allowDefaultSeed = _configuration.GetValue<bool>("Bootstrap:SeedDefaultInviteCodes");
        var inviteCodes = configuredInviteCodes;

        if (inviteCodes.Length == 0 && allowDefaultSeed && (_environment.IsDevelopment() || _environment.IsEnvironment("Testing")))
        {
            inviteCodes = ["ALPHA-OPEN-2026"];
        }

        if (inviteCodes.Length == 0)
        {
            _logger.LogWarning("No bootstrap invite codes were seeded.");
            return;
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var code in inviteCodes.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).Distinct())
        {
            _dbContext.Invites.Add(new Invite
            {
                Id = Guid.NewGuid(),
                CodeHash = HashInviteCode(code),
                CreatedAt = now,
                ExpiresAt = now.AddYears(1),
                MaxUses = 10_000
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {InviteCount} bootstrap invite codes.", inviteCodes.Length);
    }

    private static string HashInviteCode(string inviteCode)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(inviteCode));
        return Convert.ToHexString(bytes);
    }
}
