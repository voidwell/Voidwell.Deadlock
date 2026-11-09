using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("match");

        builder.HasKey(a => new { a.StartTime, a.MatchId });

        builder.Property(a => a.StartTime).HasColumnType("timestamp with time zone");
        builder.Property(a => a.MatchMode).HasConversion<short>();
        builder.Property(a => a.WinningTeam).HasConversion<short>();

        builder.HasIndex(a => a.MatchId);
    }
}
