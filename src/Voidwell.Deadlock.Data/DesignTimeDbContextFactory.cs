using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Voidwell.Deadlock.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DeadlockDbContext>
{
    public DeadlockDbContext CreateDbContext(string[] args)
    {
        // Settings live in the Api project. A configuration file path outside the base path is ignored, so these
        // are given as absolute paths.
        var apiDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Voidwell.Deadlock.Api"));

        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile(Path.Combine(apiDirectory, "appsettings.json"), optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile(Path.Combine(apiDirectory, "appsettings.Development.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();

        var options = configuration.Get<DatabaseOptions>() ?? new DatabaseOptions();
        options.CommandTimeout ??= 180;

        return new DeadlockDbContext(DeadlockDbContext.CreateDefaultDbContextOptions(options));
    }
}
