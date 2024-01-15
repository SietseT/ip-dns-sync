using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PublicDnsUpdater.Configuration;
using PublicDnsUpdater.Providers.TransIP;
using PublicDnsUpdater.Providers.TransIP.Abstractions;
using PublicDnsUpdater.Providers.TransIP.Requests;

namespace PublicDnsUpdater.Workers;

public class UpdaterWorker(
    IHttpClientFactory httpClientFactory,
    ITransIpClient transIpClient,
    IOptions<ProviderConfiguration<TransIpConfiguration>> transIpConfiguration)
    : IHostedService, IDisposable
{
    private readonly ProviderConfiguration<TransIpConfiguration> _transIpConfiguration = transIpConfiguration.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Run(cancellationToken);
        
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            await Run(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task Run(CancellationToken cancellationToken)
    {
        var externalIp = await GetExternalIp(cancellationToken);

        foreach (var domain in _transIpConfiguration.Domains)
        {
            var dnsEntriesResponse = await transIpClient.GetDnsEntriesAsync(domain, cancellationToken);

            if (!dnsEntriesResponse.WasSuccessful)
            {
                //todo log
                continue;
            }
            
            foreach (var entry in dnsEntriesResponse.Data.DnsEntries.Where(dnsEntry => dnsEntry.Type == "A" && dnsEntry.Content != externalIp))
            {
                await UpdateExternalIp(domain, entry, externalIp, cancellationToken);
            }
        }
    }
    
    private async Task<string> GetExternalIp(CancellationToken cancellationToken)
    {
        var externalIpClient = httpClientFactory.CreateClient();
        return await externalIpClient.GetStringAsync("https://api.ipify.org", cancellationToken);
    }

    private async Task UpdateExternalIp(string domain, DnsEntry dnsEntry, string externalIp,
        CancellationToken cancellationToken)
    {
        dnsEntry.Content = externalIp;
        var request = new UpdateDnsEntryRequest
        {
            DnsEntry = dnsEntry
        };
        
        var updateDnsEntryResponse = await transIpClient.UpdateDnsEntryAsync(domain, request, cancellationToken);
        
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}