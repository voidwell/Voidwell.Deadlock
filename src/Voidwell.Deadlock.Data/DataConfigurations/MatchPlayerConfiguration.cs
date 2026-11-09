using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class MatchPlayerConfiguration : IEntityTypeConfiguration<MatchPlayer>
{
    public void Configure(EntityTypeBuilder<MatchPlayer> builder)
    {
        builder.ToTable("match_player");

        builder.HasKey(a => new { a.StartTime, a.MatchId, a.PlayerSlot });

        builder.Property(a => a.StartTime).HasColumnType("timestamp with time zone");
        builder.Property(a => a.Team).HasConversion<short>();
        builder.Property(a => a.PlayerMatchOutcome).HasConversion<short>();
        builder.Property(a => a.MatchMode).HasConversion<short>();

        builder.HasIndex(a => a.MatchId);
        builder.HasIndex(a => new { a.HeroId, a.StartTime });
    }
}
