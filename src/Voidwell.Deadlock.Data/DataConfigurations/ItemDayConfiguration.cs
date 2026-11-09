using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class ItemDayConfiguration : IEntityTypeConfiguration<ItemDay>
{
    public void Configure(EntityTypeBuilder<ItemDay> builder)
    {
        builder.ToTable("item_day");

        builder.HasKey(a => new { a.Day, a.Mode, a.Tier, a.ItemId });
    }
}
