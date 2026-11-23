namespace Voidwell.Deadlock.Analytics;

/// <summary>
/// One player's shop items in one match. <see cref="Items"/> are item indexes (0 to item count - 1), each once, and
/// <see cref="BuyTimesS"/> holds the time of the first purchase of each.
/// </summary>
public sealed record ItemBuildGame(bool Won, int DurationS, int[] Items, int[] BuyTimesS);

public sealed class ItemBuildOptions
{
    /// <summary>
    /// The most items the build can have.
    /// </summary>
    public int MaxItems { get; init; } = 12;

    /// <summary>
    /// The fewest games an item must have been bought in, and not bought in, to be compared.
    /// </summary>
    public int MinGames { get; init; } = 100;

    /// <summary>
    /// Items bought in at least this share of games are core items: nearly everyone buys them, so there are too few
    /// games without them to measure them, and they go in the build regardless.
    /// </summary>
    public double CorePickRate { get; init; } = 0.7;
}

public enum ItemBuildStopReason
{
    /// <summary>
    /// The build reached the maximum number of items.
    /// </summary>
    MaxItems,

    /// <summary>
    /// No further item had enough games, alongside the items already chosen, to be compared.
    /// </summary>
    NotEnoughGames,

    /// <summary>
    /// No further item raised the win rate alongside the items already chosen by more than chance could explain: the
    /// lower end of the 95% interval of every item's lift was zero or below.
    /// </summary>
    NoBetterItem
}

/// <summary>
/// An item in a build and how it was chosen. Rates compare the games in the item's cohort (games that had every item
/// chosen before it) where the item was bought against those where it was not bought although the game lasted until
/// the time players usually buy it.
/// </summary>
public sealed record ItemBuildPick(
    int Item,
    int Pick,
    bool Core,
    int CohortGames,
    int Games,
    int Wins,
    int GamesWithout,
    int WinsWithout,
    int MedianBuyTimeS,
    double Lift,
    double LiftLow,
    double StandaloneLift);

/// <param name="Items">The build in the order to buy it.</param>
/// <param name="FullBuildGames">Games where every non-core item of the build was bought.</param>
/// <param name="FullBuildWins">Wins among <paramref name="FullBuildGames"/>.</param>
/// <param name="StopReason">Why the build has no more items.</param>
public sealed record ItemBuildResult(IReadOnlyList<ItemBuildPick> Items, int FullBuildGames, int FullBuildWins, ItemBuildStopReason StopReason);

/// <summary>
/// Finds the shop items that give a hero the best odds of winning when bought together.
/// </summary>
/// <remarks>
/// <para>
/// The build is chosen one item at a time. Each step looks only at the games that had every item chosen so far, and
/// picks the item that raised the win rate most within them. Because each item is judged alongside the earlier ones, an
/// item that works well with them ranks higher than one that only does well on its own: that is how synergy is found.
/// </para>
/// <para>
/// A plain win rate favours late, expensive items, because only players who are ahead and whose games last get to buy
/// them. So an item is compared with the games where it was not bought but which lasted at least until the time it is
/// usually bought, and ranked by the lower end of the interval of the difference, so an item seen in few games does not
/// win on luck. Each step compares every item at once, and among a hundred or so one usually looks good by chance, so the
/// interval is widened for the number compared (a Bonferroni correction): there is a 95% chance that none of the items
/// compared at a step is outside its interval. The build ends when no item's interval is clear of zero. This removes the
/// game length bias but not all of the gold bias: players who are ahead still buy more. Items bought by nearly everyone cannot be compared at all, so they are added as core items without narrowing
/// the games.
/// </para>
/// </remarks>
public static class ItemBuildOptimizer
{
    /// <summary>
    /// The chance, at each step, that any item compared is wrongly judged better or worse than its true lift.
    /// </summary>
    public const double FamilyErrorRate = 0.05;

    public static ItemBuildResult Find(IReadOnlyList<ItemBuildGame> games, int itemCount, ItemBuildOptions options)
    {
        var all = Enumerable.Range(0, games.Count).ToArray();
        var standalone = Evaluate(games, all, itemCount);
        var picks = new List<ItemBuildPick>();
        var chosen = new HashSet<int>();

        foreach (var candidate in standalone
            .Where(a => a.Games >= options.CorePickRate * games.Count && a.Games > 0)
            .OrderByDescending(a => a.Games)
            .Take(options.MaxItems))
        {
            picks.Add(ToPick(candidate, picks.Count + 1, core: true, games.Count, CriticalZ(1), candidate.Lift));
            chosen.Add(candidate.Item);
        }

        var cohort = all;
        var stopReason = ItemBuildStopReason.MaxItems;

        while (picks.Count < options.MaxItems)
        {
            var compared = Evaluate(games, cohort, itemCount)
                .Where(a => !chosen.Contains(a.Item) && a.Games >= options.MinGames && a.GamesWithout >= options.MinGames)
                .ToList();

            if (compared.Count == 0)
            {
                stopReason = ItemBuildStopReason.NotEnoughGames;
                break;
            }

            var z = CriticalZ(compared.Count);
            var best = compared
                .OrderByDescending(a => a.Lift - (z * a.StandardError))
                .ThenByDescending(a => a.Games)
                .First();

            // Some item always looks a little better by chance, so only an item whose lift is clear of zero is added.
            if (best.Lift - (z * best.StandardError) <= 0)
            {
                stopReason = ItemBuildStopReason.NoBetterItem;
                break;
            }

            picks.Add(ToPick(best, picks.Count + 1, core: false, cohort.Length, z, standalone[best.Item].Lift));
            chosen.Add(best.Item);
            cohort = cohort.Where(a => Array.IndexOf(games[a].Items, best.Item) >= 0).ToArray();
        }

        var ordered = picks
            .OrderBy(a => a.MedianBuyTimeS)
            .ThenBy(a => a.Pick)
            .ToList();

        return new ItemBuildResult(ordered, cohort.Length, cohort.Count(a => games[a].Won), stopReason);
    }

    /// <summary>
    /// The z of a two-sided interval wide enough that all <paramref name="compared"/> items stay inside theirs with
    /// 95% probability.
    /// </summary>
    public static double CriticalZ(int compared)
    {
        return Statistics.NormalQuantile(1 - (FamilyErrorRate / (2 * Math.Max(1, compared))));
    }

    private static ItemBuildPick ToPick(Candidate candidate, int pick, bool core, int cohortGames, double z, double standaloneLift)
    {
        return new ItemBuildPick(
            candidate.Item,
            pick,
            core,
            cohortGames,
            candidate.Games,
            candidate.Wins,
            candidate.GamesWithout,
            candidate.WinsWithout,
            candidate.MedianBuyTimeS,
            candidate.Lift,
            candidate.Lift - (z * candidate.StandardError),
            standaloneLift);
    }

    /// <summary>
    /// Compares every item within the cohort. Returns one candidate per item index.
    /// </summary>
    private static Candidate[] Evaluate(IReadOnlyList<ItemBuildGame> games, int[] cohort, int itemCount)
    {
        // Durations sorted, with the wins of every game from each position on, give the games that lasted at least as
        // long as any time in one binary search.
        var durations = new int[cohort.Length];
        var winsFrom = new int[cohort.Length + 1];
        var byDuration = cohort.OrderBy(a => games[a].DurationS).ToArray();

        for (var i = byDuration.Length - 1; i >= 0; i--)
        {
            durations[i] = games[byDuration[i]].DurationS;
            winsFrom[i] = winsFrom[i + 1] + (games[byDuration[i]].Won ? 1 : 0);
        }

        var buyers = new List<Buyer>?[itemCount];

        foreach (var index in cohort)
        {
            var game = games[index];

            for (var i = 0; i < game.Items.Length; i++)
            {
                (buyers[game.Items[i]] ??= []).Add(new Buyer(game.BuyTimesS[i], game.DurationS, game.Won));
            }
        }

        var candidates = new Candidate[itemCount];

        for (var item = 0; item < itemCount; item++)
        {
            var itemBuyers = buyers[item];
            if (itemBuyers == null)
            {
                candidates[item] = new Candidate(item, 0, 0, 0, 0, 0, 0, double.PositiveInfinity);
                continue;
            }

            itemBuyers.Sort((a, b) => a.BuyTimeS.CompareTo(b.BuyTimeS));
            var medianBuyTimeS = itemBuyers[itemBuyers.Count / 2].BuyTimeS;

            var wins = 0;
            var buyersLasting = 0;
            var buyerWinsLasting = 0;

            foreach (var buyer in itemBuyers)
            {
                wins += buyer.Won ? 1 : 0;

                if (buyer.DurationS >= medianBuyTimeS)
                {
                    buyersLasting++;
                    buyerWinsLasting += buyer.Won ? 1 : 0;
                }
            }

            // Games that lasted until the usual buy time, less the buyers among them, are the ones that could have bought
            // the item and did not.
            var from = LowerBound(durations, medianBuyTimeS);
            var gamesWithout = durations.Length - from - buyersLasting;
            var winsWithout = winsFrom[from] - buyerWinsLasting;

            var (lift, standardError) = Difference(wins, itemBuyers.Count, winsWithout, gamesWithout);

            candidates[item] = new Candidate(item, itemBuyers.Count, wins, gamesWithout, winsWithout, medianBuyTimeS, lift, standardError);
        }

        return candidates;
    }

    /// <summary>
    /// The difference between two win rates and its standard error.
    /// </summary>
    public static (double Lift, double StandardError) Difference(long wins, long games, long winsWithout, long gamesWithout)
    {
        if (games == 0 || gamesWithout == 0)
        {
            return (0, double.PositiveInfinity);
        }

        var rate = (double)wins / games;
        var rateWithout = (double)winsWithout / gamesWithout;
        var standardError = Math.Sqrt((rate * (1 - rate) / games) + (rateWithout * (1 - rateWithout) / gamesWithout));
        return (rate - rateWithout, standardError);
    }

    private static int LowerBound(int[] sorted, int value)
    {
        int low = 0, high = sorted.Length;

        while (low < high)
        {
            var mid = (low + high) / 2;
            if (sorted[mid] < value)
            {
                low = mid + 1;
            }
            else
            {
                high = mid;
            }
        }

        return low;
    }

    private readonly record struct Buyer(int BuyTimeS, int DurationS, bool Won);

    private sealed record Candidate(int Item, int Games, int Wins, int GamesWithout, int WinsWithout, int MedianBuyTimeS, double Lift, double StandardError);
}
