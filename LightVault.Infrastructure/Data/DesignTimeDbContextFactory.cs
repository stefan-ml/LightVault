using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LightVault.Infrastructure.Data;

public class LightVaultDbContextFactory
    : IDesignTimeDbContextFactory<LightVaultDbContext>
{
    public LightVaultDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..",
            "LightVault.API"
        );

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.Development.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            config.GetConnectionString("Default")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string not found!");

        var options = new DbContextOptionsBuilder<LightVaultDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new LightVaultDbContext(options);
    }
}
