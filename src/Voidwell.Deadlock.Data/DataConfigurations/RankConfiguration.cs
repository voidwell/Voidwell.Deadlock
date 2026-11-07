using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class RankConfiguration : IEntityTypeConfiguration<Rank>
{
    public void Configure(EntityTypeBuilder<Rank> builder)
    {
        builder.ToTable("rank");

        builder.HasKey(a => a.Tier);

        builder.Property(a => a.Tier).ValueGeneratedNever();
    }
}
