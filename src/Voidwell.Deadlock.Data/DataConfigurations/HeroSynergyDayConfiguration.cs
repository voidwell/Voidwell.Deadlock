using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class HeroSynergyDayConfiguration : IEntityTypeConfiguration<HeroSynergyDay>
{
    public void Configure(EntityTypeBuilder<HeroSynergyDay> builder)
    {
        builder.ToTable("hero_synergy_day");

        builder.HasKey(a => new { a.HeroId, a.Day, a.Mode, a.Tier, a.AllyHeroId });
    }
}
