using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class HeroCurveDayConfiguration : IEntityTypeConfiguration<HeroCurveDay>
{
    public void Configure(EntityTypeBuilder<HeroCurveDay> builder)
    {
        builder.ToTable("hero_curve_day");

        builder.HasKey(a => new { a.HeroId, a.Day, a.Mode, a.Tier, a.Won, a.TimeStampS });
    }
}
