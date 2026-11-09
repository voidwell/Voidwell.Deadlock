using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class IngestFileConfiguration : IEntityTypeConfiguration<IngestFile>
{
    public void Configure(EntityTypeBuilder<IngestFile> builder)
    {
        builder.ToTable("ingest_file");

        builder.HasKey(a => a.Key);

        builder.Property(a => a.IngestedAt).HasColumnType("timestamp with time zone");
    }
}
