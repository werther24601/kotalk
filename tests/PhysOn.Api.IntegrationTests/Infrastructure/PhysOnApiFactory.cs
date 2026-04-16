using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace PhysOn.Api.IntegrationTests.Infrastructure;

public sealed class PhysOnApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"vsmessenger-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        var contentRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/PhysOn.Api"));
        builder.UseContentRoot(contentRoot);
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["ConnectionStrings:Main"] = $"Data Source={_databasePath}",
                ["Bootstrap:SeedDefaultInviteCodes"] = "true",
                ["Bootstrap:InviteCodes:0"] = "ALPHA-OPEN-2026",
                ["Auth:Jwt:SigningKey"] = "testing-signing-key-should-never-ship-2026-very-secret"
            };

            configBuilder.AddInMemoryCollection(overrides);
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();

        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}
