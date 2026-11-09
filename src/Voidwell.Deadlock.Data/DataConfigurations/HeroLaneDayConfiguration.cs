using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class HeroLaneDayConfiguration : IEntityTypeConfiguration<HeroLaneDay>
{
    public void Configure(EntityTypeBuilder<HeroLaneDay> builder)
    {
        builder.ToTable("hero_lane_day");

        builder.HasKey(a => new { a.HeroId, a.Day, a.Mode, a.Tier, a.EnemyHeroId });
    }
}
