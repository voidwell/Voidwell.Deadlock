using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class HeroBanDayConfiguration : IEntityTypeConfiguration<HeroBanDay>
{
    public void Configure(EntityTypeBuilder<HeroBanDay> builder)
    {
        builder.ToTable("hero_ban_day");

        builder.HasKey(a => new { a.Day, a.Mode, a.Tier, a.HeroId });
    }
}
