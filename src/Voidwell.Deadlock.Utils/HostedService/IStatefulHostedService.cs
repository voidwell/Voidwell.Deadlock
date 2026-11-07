using Microsoft.Extensions.Hosting;

namespace Voidwell.Deadlock.Utils.HostedService;

public interface IStatefulHostedService : IHostedService
{
    Task OnApplicationStartup(CancellationToken cancellationToken);
    Task OnApplicationShutdown(CancellationToken cancellationToken);
    Task<object> GetStatusAsync(CancellationToken cancellationToken);
}
