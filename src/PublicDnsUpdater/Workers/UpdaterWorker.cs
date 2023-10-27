using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PublicDnsUpdater.Configuration;
using PublicDnsUpdater.Helpers;
using PublicDnsUpdater.Providers.TransIP;
using PublicDnsUpdater.Providers.TransIP.Requests;
using PublicDnsUpdater.Providers.TransIP.Responses;

namespace PublicDnsUpdater.Workers;

public class UpdaterWorker : IHostedService, IDisposable
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ProviderConfiguration<TransIpConfiguration> _transIpConfiguration;

    public UpdaterWorker(IHttpClientFactory httpClientFactory, IOptions<ProviderConfiguration<TransIpConfiguration>> transIpConfiguration)
    {
        _httpClientFactory = httpClientFactory;
        _transIpConfiguration = transIpConfiguration.Value;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Run(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private async Task Run(CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Provider.TransIp);

        var externalIp = await GetExternalIp(cancellationToken);

        foreach (var domain in _transIpConfiguration.Domains)
        {
            var dnsEntriesRequest = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://api.transip.nl/v6/domains/{domain}/dns"));
            var dnsEntriesResponse = await httpClient.SendAsync(dnsEntriesRequest, cancellationToken);
            var dnsEntriesResponseBody = await dnsEntriesResponse.Content.ReadAsStringAsync(cancellationToken);
            var getDnsEntries = JsonSerializer.Deserialize<GetDnsEntriesResponse>(dnsEntriesResponseBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (getDnsEntries == null)
            {
                // TODO: log
                continue;
            }
            
            foreach (var entry in getDnsEntries.DnsEntries.Where(dnsEntry => dnsEntry.Type == "A" && dnsEntry.Content != externalIp))
            {
                await UpdateExternalIp(domain, entry, externalIp);
            }
        }
    }
    
    private async Task<string> GetExternalIp(CancellationToken cancellationToken)
    {
        var externalIpClient = _httpClientFactory.CreateClient();
        return await externalIpClient.GetStringAsync("https://api.ipify.org", cancellationToken);
    }

    private async Task UpdateExternalIp(string domain, DnsEntry dnsEntry, string externalIp)
    {
        dnsEntry.Content = externalIp;
        var requestBody = new UpdateDnsEntryRequest
        {
            DnsEntry = dnsEntry
        };
        
        var dnsEntriesRequest = new HttpRequestMessage(HttpMethod.Patch, new Uri($"https://api.transip.nl/v6/domains/{domain}/dns"));
        var body = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        dnsEntriesRequest.Content = new StringContent(body);
        
        var response = await _httpClientFactory.CreateClient(Provider.TransIp).SendAsync(dnsEntriesRequest);
        if(response.IsSuccessStatusCode) return;

        var responseBody = await response.Content.ReadAsStringAsync();
        // TODO: log
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}