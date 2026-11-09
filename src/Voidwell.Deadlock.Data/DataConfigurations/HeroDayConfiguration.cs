using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class HeroDayConfiguration : IEntityTypeConfiguration<HeroDay>
{
    public void Configure(EntityTypeBuilder<HeroDay> builder)
    {
        builder.ToTable("hero_day");

        builder.HasKey(a => new { a.Day, a.Mode, a.Tier, a.HeroId });

        // Hero pages read one hero across a date range.
        builder.HasIndex(a => new { a.HeroId, a.Day });
    }
}
