using Voidwell.Deadlock.Data.Models;

namespace Voidwell.Deadlock.Data.Repositories.Abstractions;

public interface IUpdaterSchedulerRepository
{
    Task<UpdaterScheduler?> GetUpdaterHistoryByServiceNameAsync(string serviceName);
    Task UpsertAsync(UpdaterScheduler entity);
}
