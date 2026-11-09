using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class MatchPlayerItemConfiguration : IEntityTypeConfiguration<MatchPlayerItem>
{
    public void Configure(EntityTypeBuilder<MatchPlayerItem> builder)
    {
        builder.ToTable("match_player_item");

        builder.HasKey(a => new { a.StartTime, a.MatchId, a.PlayerSlot, a.Seq });

        builder.Property(a => a.StartTime).HasColumnType("timestamp with time zone");

        builder.HasIndex(a => new { a.ItemId, a.StartTime });
    }
}
