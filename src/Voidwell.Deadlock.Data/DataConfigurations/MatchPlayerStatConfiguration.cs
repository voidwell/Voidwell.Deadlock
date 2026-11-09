using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class MatchPlayerStatConfiguration : IEntityTypeConfiguration<MatchPlayerStat>
{
    public void Configure(EntityTypeBuilder<MatchPlayerStat> builder)
    {
        builder.ToTable("match_player_stat");

        builder.HasKey(a => new { a.StartTime, a.MatchId, a.PlayerSlot, a.TimeStampS });

        builder.Property(a => a.StartTime).HasColumnType("timestamp with time zone");
    }
}
