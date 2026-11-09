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
    public DbSet<Patch> Patches => Set<Patch>();

    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchPlayer> MatchPlayers => Set<MatchPlayer>();
    public DbSet<MatchPlayerItem> MatchPlayerItems => Set<MatchPlayerItem>();
    public DbSet<MatchPlayerStat> MatchPlayerStats => Set<MatchPlayerStat>();
    public DbSet<IngestFile> IngestFiles => Set<IngestFile>();

    public DbSet<MatchDay> MatchDays => Set<MatchDay>();
    public DbSet<HeroDay> HeroDays => Set<HeroDay>();
    public DbSet<HeroBanDay> HeroBanDays => Set<HeroBanDay>();
    public DbSet<HeroItemDay> HeroItemDays => Set<HeroItemDay>();
    public DbSet<ItemDay> ItemDays => Set<ItemDay>();
    public DbSet<HeroBuildShapeDay> HeroBuildShapeDays => Set<HeroBuildShapeDay>();
    public DbSet<HeroBuildDay> HeroBuildDays => Set<HeroBuildDay>();
    public DbSet<HeroAbilityDay> HeroAbilityDays => Set<HeroAbilityDay>();
    public DbSet<HeroAbilityPickDay> HeroAbilityPickDays => Set<HeroAbilityPickDay>();
    public DbSet<HeroMatchupDay> HeroMatchupDays => Set<HeroMatchupDay>();
    public DbSet<HeroSynergyDay> HeroSynergyDays => Set<HeroSynergyDay>();
    public DbSet<HeroLaneDay> HeroLaneDays => Set<HeroLaneDay>();
    public DbSet<TeamCompositionDay> TeamCompositionDays => Set<TeamCompositionDay>();
    public DbSet<HeroCurveDay> HeroCurveDays => Set<HeroCurveDay>();
    public DbSet<TeamLeadDay> TeamLeadDays => Set<TeamLeadDay>();
    public DbSet<MatchHourDay> MatchHourDays => Set<MatchHourDay>();
    public DbSet<MatchDurationDay> MatchDurationDays => Set<MatchDurationDay>();
    public DbSet<MatchObjectiveDay> MatchObjectiveDays => Set<MatchObjectiveDay>();
    public DbSet<MatchLeaverDay> MatchLeaverDays => Set<MatchLeaverDay>();

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
