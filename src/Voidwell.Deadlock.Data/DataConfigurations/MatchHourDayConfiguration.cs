using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class MatchHourDayConfiguration : IEntityTypeConfiguration<MatchHourDay>
{
    public void Configure(EntityTypeBuilder<MatchHourDay> builder)
    {
        builder.ToTable("match_hour_day");

        builder.HasKey(a => new { a.Day, a.Mode, a.Tier, a.Hour });
    }
}
