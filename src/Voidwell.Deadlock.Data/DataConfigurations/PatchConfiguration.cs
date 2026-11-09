using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class PatchConfiguration : IEntityTypeConfiguration<Patch>
{
    public void Configure(EntityTypeBuilder<Patch> builder)
    {
        builder.ToTable("patch");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.PublishedAt).HasColumnType("timestamp with time zone");

        builder.HasIndex(a => a.PublishedAt);
    }
}
