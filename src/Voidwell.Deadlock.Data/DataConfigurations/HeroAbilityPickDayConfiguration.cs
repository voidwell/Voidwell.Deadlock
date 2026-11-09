using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class HeroAbilityPickDayConfiguration : IEntityTypeConfiguration<HeroAbilityPickDay>
{
    public void Configure(EntityTypeBuilder<HeroAbilityPickDay> builder)
    {
        builder.ToTable("hero_ability_pick_day");

        // Hero pages read one hero over a date range, so the hero comes first in the key.
        builder.HasKey(a => new { a.HeroId, a.Day, a.Mode, a.Tier, a.PickIndex, a.AbilityId });
    }
}
