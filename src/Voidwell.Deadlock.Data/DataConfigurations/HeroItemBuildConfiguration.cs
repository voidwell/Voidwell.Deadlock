using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class HeroItemBuildConfiguration : IEntityTypeConfiguration<HeroItemBuild>
{
    public void Configure(EntityTypeBuilder<HeroItemBuild> builder)
    {
        builder.ToTable("hero_item_build");

        // Builds are read one hero over a date range, so the hero comes first in the key.
        builder.HasKey(a => new { a.HeroId, a.Day, a.MatchId, a.PlayerSlot });
    }
}
