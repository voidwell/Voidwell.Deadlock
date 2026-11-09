using Voidwell.Deadlock.Analytics.Models;

namespace Voidwell.Deadlock.Analytics;

public static class AbilityOrder
{
    /// <summary>
    /// An ability takes four points: the unlock and three upgrades.
    /// </summary>
    public const int PointsPerAbility = 4;

    /// <summary>
    /// Builds a level-up order by taking, at each pick, the ability players chose most often, without giving any ability
    /// more than four points. It is a summary of what players do, not a guarantee that any one player did this.
    /// </summary>
    public static List<long> Suggest(IReadOnlyList<AbilityPickPoint> picks)
    {
        var order = new List<long>();
        var used = new Dictionary<long, int>();

        foreach (var pick in picks.OrderBy(a => a.Pick))
        {
            var choice = pick.Options
                .Where(a => used.GetValueOrDefault(a.AbilityId) < PointsPerAbility)
                .OrderByDescending(a => a.Games)
                .FirstOrDefault();

            if (choice == null)
            {
                continue;
            }

            order.Add(choice.AbilityId);
            used[choice.AbilityId] = used.GetValueOrDefault(choice.AbilityId) + 1;
        }

        return order;
    }
}
