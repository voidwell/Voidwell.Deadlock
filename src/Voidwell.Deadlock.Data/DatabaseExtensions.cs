using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Voidwell.Deadlock.Data.Repositories;
using Voidwell.Deadlock.Data.Repositories.Abstractions;

namespace Voidwell.Deadlock.Data;

public static class DatabaseExtensions
{
    public static IServiceCollection AddEntityFrameworkContext(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.Get<DatabaseOptions>() ?? new DatabaseOptions();

        services.AddPooledDbContextFactory<DeadlockDbContext>(
            builder => DeadlockDbContext.Configure(builder, options),
            options.PoolSize);

        services.AddSingleton<IUpdaterSchedulerRepository, UpdaterSchedulerRepository>();
        services.AddSingleton<IHeroRepository, HeroRepository>();
        services.AddSingleton<IItemRepository, ItemRepository>();
        services.AddSingleton<IRankRepository, RankRepository>();

        return services;
    }

    public static IApplicationBuilder InitializeDatabases(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();

        var dbContextFactory = serviceScope.ServiceProvider.GetRequiredService<IDbContextFactory<DeadlockDbContext>>();
        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.Database.Migrate();

        return app;
    }
}
