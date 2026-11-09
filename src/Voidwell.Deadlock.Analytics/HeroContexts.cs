using Voidwell.Deadlock.Analytics.Models;
using Voidwell.Deadlock.CollectionStore.Services.Abstractions;
using Voidwell.Deadlock.Data.Repositories.Abstractions;
using Voidwell.Deadlock.Data.Repositories.Models;

namespace Voidwell.Deadlock.Analytics;

internal static class HeroContexts
{
    /// <summary>
    /// The hero's name and overall result in the query, or null when it has no games in it.
    /// </summary>
    public static async Task<HeroContext?> LoadAsync(IHeroAnalyticsRepository repository, IHeroStore heroStore, int heroId, AnalyticsFilter filter)
    {
        var totals = (await repository.GetHeroTotalsAsync(filter)).FirstOrDefault(a => a.HeroId == heroId);
        if (totals == null || totals.Games == 0)
        {
            return null;
        }

        var hero = (await heroStore.GetAllHeroes())?.FirstOrDefault(a => a.Id == heroId);

        return new HeroContext(heroId, hero?.Name, hero?.IconUrl, (int)totals.Games, Statistics.Divide(totals.Wins, totals.Games));
    }
}
