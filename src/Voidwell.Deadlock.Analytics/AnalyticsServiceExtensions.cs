using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Voidwell.Deadlock.Utils.HostedService;

namespace Voidwell.Deadlock.Analytics;

public static class AnalyticsServiceExtensions
{
    public static IServiceCollection AddAnalytics(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RollupOptions>(configuration);

        services.AddSingleton<IAnalyticsQueryResolver, AnalyticsQueryResolver>();
        services.AddSingleton<IHeroAnalyticsService, HeroAnalyticsService>();
        services.AddSingleton<IItemAnalyticsService, ItemAnalyticsService>();
        services.AddSingleton<IMatchupAnalyticsService, MatchupAnalyticsService>();
        services.AddSingleton<ICurveAnalyticsService, CurveAnalyticsService>();
        services.AddSingleton<IMatchAnalyticsService, MatchAnalyticsService>();
        services.AddSingleton<IRollupService, RollupService>();

        services.AddStatefulHostedService<RollupUpdater>();

        return services;
    }
}
