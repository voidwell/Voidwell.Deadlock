using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class TeamCompositionDayConfiguration : IEntityTypeConfiguration<TeamCompositionDay>
{
    public void Configure(EntityTypeBuilder<TeamCompositionDay> builder)
    {
        builder.ToTable("team_composition_day");

        builder.HasKey(a => new { a.Day, a.Mode, a.Tier, a.Brawlers, a.Assassins, a.Marksmen, a.Mystics, a.Supports });
    }
}
