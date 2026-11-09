using Voidwell.Deadlock.Data.Models;
using Xunit;

namespace Voidwell.Deadlock.Analytics.Test;

public class AnalyticsRangesTest
{
    private static readonly DateOnly _latest = new(2026, 11, 7);

    private static Patch PatchOn(int year, int month, int day, bool bigDay = false)
    {
        return new Patch
        {
            Id = $"{year}-{month}-{day}",
            PublishedAt = new DateTime(year, month, day, 20, 30, 0, DateTimeKind.Utc),
            IsBigDay = bigDay
        };
    }

    private static readonly Patch[] _patches =
    [
        PatchOn(2026, 9, 29, bigDay: true),
        PatchOn(2026, 10, 13),
        PatchOn(2026, 10, 30, bigDay: true)
    ];

    [Fact]
    public void FromDates_WithNothing_IsTheLastThirtyDays()
    {
        var (from, to) = AnalyticsRanges.FromDates(null, null, null, _latest);

        Assert.Equal(new DateOnly(2026, 10, 9), from);
        Assert.Equal(_latest, to);
    }

    [Fact]
    public void FromDates_WithDays_EndsAtTheLatestDay()
    {
        var (from, to) = AnalyticsRanges.FromDates(null, null, 7, _latest);

        Assert.Equal(new DateOnly(2026, 11, 1), from);
        Assert.Equal(_latest, to);
    }

    [Fact]
    public void FromDates_WithDaysAndTo_EndsAtTo()
    {
        var (from, to) = AnalyticsRanges.FromDates(null, new DateOnly(2026, 10, 20), 3, _latest);

        Assert.Equal(new DateOnly(2026, 10, 18), from);
        Assert.Equal(new DateOnly(2026, 10, 20), to);
    }

    [Fact]
    public void FromDates_WithFromOnly_RunsToTheLatestDay()
    {
        var (from, to) = AnalyticsRanges.FromDates(new DateOnly(2026, 11, 1), null, null, _latest);

        Assert.Equal(new DateOnly(2026, 11, 1), from);
        Assert.Equal(_latest, to);
    }

    [Fact]
    public void FromDates_WithSingleDay_IsOneDay()
    {
        var day = new DateOnly(2026, 10, 19);

        Assert.Equal((day, day), AnalyticsRanges.FromDates(day, day, null, _latest));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(3651)]
    public void FromDates_RejectsDaysOutOfRange(int days)
    {
        Assert.Throws<AnalyticsRequestException>(() => AnalyticsRanges.FromDates(null, null, days, _latest));
    }

    [Fact]
    public void FromDates_RejectsFromAfterTo()
    {
        Assert.Throws<AnalyticsRequestException>(() => AnalyticsRanges.FromDates(new DateOnly(2026, 11, 5), new DateOnly(2026, 11, 1), null, _latest));
    }

    [Fact]
    public void FromDates_RejectsFromWithDays()
    {
        Assert.Throws<AnalyticsRequestException>(() => AnalyticsRanges.FromDates(new DateOnly(2026, 11, 1), null, 5, _latest));
    }

    [Fact]
    public void FromPatch_CoversTheDaysBetweenTwoReleases()
    {
        var (from, to) = AnalyticsRanges.FromPatch(_patches, "2026-10-13", false, _latest);

        Assert.Equal(new DateOnly(2026, 10, 14), from);
        Assert.Equal(new DateOnly(2026, 10, 29), to);
    }

    [Fact]
    public void FromPatch_ForTheNewestPatch_RunsToTheLatestDay()
    {
        var (from, to) = AnalyticsRanges.FromPatch(_patches, "latest", false, _latest);

        Assert.Equal(new DateOnly(2026, 10, 31), from);
        Assert.Equal(_latest, to);
    }

    [Fact]
    public void FromPatch_MajorOnly_SkipsMinorPatchesAsBoundaries()
    {
        var (from, to) = AnalyticsRanges.FromPatch(_patches, "2026-09-29", true, _latest);

        Assert.Equal(new DateOnly(2026, 9, 30), from);
        Assert.Equal(new DateOnly(2026, 10, 29), to);
    }

    [Fact]
    public void FromPatch_MajorOnly_RejectsAMinorPatchDate()
    {
        Assert.Throws<AnalyticsRequestException>(() => AnalyticsRanges.FromPatch(_patches, "2026-10-13", true, _latest));
    }

    [Fact]
    public void FromPatch_IsCappedAtTheLatestDay()
    {
        var patches = new[] { PatchOn(2026, 11, 1) };

        var (from, to) = AnalyticsRanges.FromPatch(patches, "2026-11-01", false, _latest);

        Assert.Equal(new DateOnly(2026, 11, 2), from);
        Assert.Equal(_latest, to);
    }

    [Fact]
    public void FromPatch_ReleasedOnTheLatestDay_HasNoFullDaysYet()
    {
        var patches = new[] { PatchOn(2026, 11, 7) };

        Assert.Throws<AnalyticsRequestException>(() => AnalyticsRanges.FromPatch(patches, "latest", false, _latest));
    }

    [Fact]
    public void FromPatch_FollowedByAReleaseTheNextDay_HasNoFullDays()
    {
        var patches = new[] { PatchOn(2026, 10, 30), PatchOn(2026, 10, 31) };

        Assert.Throws<AnalyticsRequestException>(() => AnalyticsRanges.FromPatch(patches, "2026-10-30", false, _latest));
    }

    [Fact]
    public void FromPatch_TwoEntriesOnTheSameDay_AreOneBoundary()
    {
        var patches = new[] { PatchOn(2026, 10, 13), PatchOn(2026, 10, 13), PatchOn(2026, 10, 30) };

        var (from, to) = AnalyticsRanges.FromPatch(patches, "2026-10-13", false, _latest);

        Assert.Equal(new DateOnly(2026, 10, 14), from);
        Assert.Equal(new DateOnly(2026, 10, 29), to);
    }

    [Theory]
    [InlineData("2020-01-01")]
    [InlineData("not a date")]
    [InlineData("10/13/2026")]
    public void FromPatch_RejectsUnknownOrMalformedPatches(string patch)
    {
        Assert.Throws<AnalyticsRequestException>(() => AnalyticsRanges.FromPatch(_patches, patch, false, _latest));
    }

    [Fact]
    public void FromPatch_WithNoPatches_Throws()
    {
        Assert.Throws<AnalyticsRequestException>(() => AnalyticsRanges.FromPatch([], "latest", false, _latest));
    }

    [Fact]
    public void Resolve_RejectsPatchWithDates()
    {
        var request = new AnalyticsRequest { Patch = "2026-10-13", Days = 5 };

        Assert.Throws<AnalyticsRequestException>(() => AnalyticsRanges.Resolve(request, _latest, _patches));
    }

    [Fact]
    public void Resolve_WithPatch_UsesThePatchRange()
    {
        var request = new AnalyticsRequest { Patch = "2026-10-13" };

        var (from, to) = AnalyticsRanges.Resolve(request, _latest, _patches);

        Assert.Equal(new DateOnly(2026, 10, 14), from);
        Assert.Equal(new DateOnly(2026, 10, 29), to);
    }
}
