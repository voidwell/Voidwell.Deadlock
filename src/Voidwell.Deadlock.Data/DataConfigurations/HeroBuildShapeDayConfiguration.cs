using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class HeroBuildShapeDayConfiguration : IEntityTypeConfiguration<HeroBuildShapeDay>
{
    public void Configure(EntityTypeBuilder<HeroBuildShapeDay> builder)
    {
        builder.ToTable("hero_build_shape_day");

        // Hero pages read one hero over a date range, so the hero comes first in the key.
        builder.HasKey(a => new { a.HeroId, a.Day, a.Mode, a.Tier, a.WeaponItems, a.SpiritItems, a.VitalityItems });
    }
}
