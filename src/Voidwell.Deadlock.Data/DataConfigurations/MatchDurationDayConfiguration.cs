using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class MatchDurationDayConfiguration : IEntityTypeConfiguration<MatchDurationDay>
{
    public void Configure(EntityTypeBuilder<MatchDurationDay> builder)
    {
        builder.ToTable("match_duration_day");

        builder.HasKey(a => new { a.Day, a.Mode, a.Tier, a.Minute });
    }
}
