using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("item");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).ValueGeneratedNever();
    }
}
