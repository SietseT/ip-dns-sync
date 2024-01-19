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
        logger.LogInformation("Started background worker for {Provider}", provider);

        await Run(cancellationToken);
        
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(settings.UpdateIntervalInMinutes));
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            await Run(cancellationToken);
        }
    }
    
    private async Task Run(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting check at configured interval");
        await service.ExecuteAsync(cancellationToken);
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