using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data;

public class DeadlockDbContext : DbContext
{
    static DeadlockDbContext()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public DeadlockDbContext(DbContextOptions<DeadlockDbContext> options)
        : base(options)
    {
    }

    public DbSet<Hero> Heroes => Set<Hero>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Rank> Ranks => Set<Rank>();

    public DbSet<UpdaterScheduler> UpdaterScheduler => Set<UpdaterScheduler>();

    internal static DbContextOptions<DeadlockDbContext> CreateDefaultDbContextOptions(DatabaseOptions options)
    {
        var builder = new DbContextOptionsBuilder<DeadlockDbContext>();

        Configure(builder, options);

        return builder.Options;
    }

    internal static void Configure(DbContextOptionsBuilder builder, DatabaseOptions options)
    {
        builder.UseNpgsql(options.ConnectionString, b =>
        {
            b.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);

            if (options.CommandTimeout != null)
            {
                b.CommandTimeout(options.CommandTimeout);
            }
        })
        .UseSnakeCaseNamingConvention()
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(DeadlockDbContext).Assembly);
    }
}
