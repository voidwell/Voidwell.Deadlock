using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class HeroMatchupDayConfiguration : IEntityTypeConfiguration<HeroMatchupDay>
{
    public void Configure(EntityTypeBuilder<HeroMatchupDay> builder)
    {
        builder.ToTable("hero_matchup_day");

        builder.HasKey(a => new { a.HeroId, a.Day, a.Mode, a.Tier, a.EnemyHeroId });
    }
}
