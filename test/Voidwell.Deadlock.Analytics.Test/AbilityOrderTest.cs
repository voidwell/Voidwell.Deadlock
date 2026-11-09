using Voidwell.Deadlock.Analytics.Models;
using Xunit;

namespace Voidwell.Deadlock.Analytics.Test;

public class AbilityOrderTest
{
    private static AbilityPickPoint Pick(int pick, params (long Ability, int Games)[] options)
    {
        var total = options.Sum(a => a.Games);
        return new AbilityPickPoint(pick, options.Select(a => new AbilityPickOption(a.Ability, null, a.Games, (double)a.Games / total, 0.5)).ToList());
    }

    [Fact]
    public void Suggest_TakesTheMostChosenAbilityAtEachPick()
    {
        var picks = new[]
        {
            Pick(1, (10, 80), (20, 20)),
            Pick(2, (10, 30), (20, 70)),
            Pick(3, (10, 60), (20, 40))
        };

        Assert.Equal([10L, 20L, 10L], AbilityOrder.Suggest(picks));
    }

    [Fact]
    public void Suggest_NeverGivesAnAbilityMoreThanFourPoints()
    {
        // Ability 10 is the most common at every pick, but can only have four points.
        var picks = Enumerable.Range(1, 6).Select(i => Pick(i, (10, 90), (20, 10))).ToArray();

        var order = AbilityOrder.Suggest(picks);

        Assert.Equal(6, order.Count);
        Assert.Equal(4, order.Count(a => a == 10));
        Assert.Equal(2, order.Count(a => a == 20));
        Assert.Equal([10L, 10L, 10L, 10L, 20L, 20L], order);
    }

    [Fact]
    public void Suggest_FollowsPickNumberRatherThanListOrder()
    {
        var picks = new[]
        {
            Pick(2, (20, 100)),
            Pick(1, (10, 100))
        };

        Assert.Equal([10L, 20L], AbilityOrder.Suggest(picks));
    }

    [Fact]
    public void Suggest_SkipsAPickWhenEveryOptionIsUsedUp()
    {
        var picks = Enumerable.Range(1, 5).Select(i => Pick(i, (10, 100))).ToArray();

        var order = AbilityOrder.Suggest(picks);

        Assert.Equal([10L, 10L, 10L, 10L], order);
    }

    [Fact]
    public void Suggest_WithNoPicks_IsEmpty()
    {
        Assert.Empty(AbilityOrder.Suggest([]));
    }
}
