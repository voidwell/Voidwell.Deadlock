using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Voidwell.Deadlock.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DeadlockDbContext>
{
    public DeadlockDbContext CreateDbContext(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile(Path.Combine("..", "Voidwell.Deadlock.Api", "appsettings.json"), optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile(Path.Combine("..", "Voidwell.Deadlock.Api", "appsettings.Development.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();

        var options = configuration.Get<DatabaseOptions>() ?? new DatabaseOptions();
        options.CommandTimeout ??= 180;

        return new DeadlockDbContext(DeadlockDbContext.CreateDefaultDbContextOptions(options));
    }
}
