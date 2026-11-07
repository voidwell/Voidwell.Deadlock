using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class HeroConfiguration : IEntityTypeConfiguration<Hero>
{
    public void Configure(EntityTypeBuilder<Hero> builder)
    {
        builder.ToTable("hero");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).ValueGeneratedNever();
    }
}
