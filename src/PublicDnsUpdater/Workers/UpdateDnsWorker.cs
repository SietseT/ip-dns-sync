using Microsoft.Extensions.Hosting;
using PublicDnsUpdater.Core.Abstractions;

namespace PublicDnsUpdater.Workers;

public class UpdateDnsWorker(IUpdateDnsService service)
    : IHostedService, IDisposable
{

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await service.ExecuteAsync(cancellationToken);
        
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            await service.ExecuteAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}