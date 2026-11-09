using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.DataConfigurations;

public class MatchObjectiveDayConfiguration : IEntityTypeConfiguration<MatchObjectiveDay>
{
    public void Configure(EntityTypeBuilder<MatchObjectiveDay> builder)
    {
        builder.ToTable("match_objective_day");

        builder.HasKey(a => new { a.Day, a.Mode, a.Tier, a.Objective });
    }
}
