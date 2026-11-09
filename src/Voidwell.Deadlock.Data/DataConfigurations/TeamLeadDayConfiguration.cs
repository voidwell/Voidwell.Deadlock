using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class TeamLeadDayConfiguration : IEntityTypeConfiguration<TeamLeadDay>
{
    public void Configure(EntityTypeBuilder<TeamLeadDay> builder)
    {
        builder.ToTable("team_lead_day");

        builder.HasKey(a => new { a.Day, a.Mode, a.Tier, a.TimeStampS, a.LeadBucket });
    }
}
