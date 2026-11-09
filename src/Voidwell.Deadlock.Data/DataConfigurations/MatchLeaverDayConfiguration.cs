using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class MatchLeaverDayConfiguration : IEntityTypeConfiguration<MatchLeaverDay>
{
    public void Configure(EntityTypeBuilder<MatchLeaverDay> builder)
    {
        builder.ToTable("match_leaver_day");

        builder.HasKey(a => new { a.Day, a.Mode, a.Tier, a.OwnLeavers, a.EnemyLeavers });
    }
}
