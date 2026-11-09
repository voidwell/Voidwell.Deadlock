using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class MatchDayConfiguration : IEntityTypeConfiguration<MatchDay>
{
    public void Configure(EntityTypeBuilder<MatchDay> builder)
    {
        builder.ToTable("match_day");

        builder.HasKey(a => new { a.Day, a.Mode, a.Tier });
    }
}
