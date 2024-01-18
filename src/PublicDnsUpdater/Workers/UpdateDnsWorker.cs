using Microsoft.Extensions.Logging;
using PublicDnsUpdater.Configuration;
using PublicDnsUpdater.Core.Abstractions;

namespace PublicDnsUpdater.Workers;

public class UpdateDnsWorker(IUpdateDnsService service, Provider provider, Settings settings, ILogger logger)
    : IUpdateDnsWorker, IDisposable
{
    public Provider Provider => provider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Started worker for {Provider}", provider);
        
        await service.ExecuteAsync(cancellationToken);
        
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(settings.UpdateIntervalInMinutes));
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            await service.ExecuteAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopped worker for {Provider}", provider);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}