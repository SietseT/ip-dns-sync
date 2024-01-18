using System.Text.Json;
using PublicDnsUpdater.Core;
using PublicDnsUpdater.Core.Abstractions;
using PublicDnsUpdater.Helpers;
using PublicDnsUpdater.Services.Dns.TransIP.Requests;
using PublicDnsUpdater.Services.Dns.TransIP.Responses;
using DnsEntry = PublicDnsUpdater.Services.Dns.TransIP;

namespace PublicDnsUpdater.Services.Dns.TransIP;

public class TransIpService(HttpClient httpClient) : IDnsProviderService
{
    public async Task<IEnumerable<Core.DnsEntry>> GetDnsEntriesForDomainAsync(string domain, CancellationToken cancellationToken)
    {
        var dnsEntriesRequest = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://api.transip.nl/v6/domains/{domain}/dns"));
        var dnsEntriesResponse = await httpClient.SendAsync(dnsEntriesRequest, cancellationToken);

        if (!dnsEntriesResponse.IsSuccessStatusCode)
        {
            var responseString = await dnsEntriesResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new DnsProviderException($"Failed to get DNS entries for domain '{domain}: {responseString}'");
        }

        var dnsEntriesResponseBody = await dnsEntriesResponse.Content.ReadAsStringAsync(cancellationToken);
        var response = JsonSerializer.Deserialize<GetDnsEntriesResponse>(dnsEntriesResponseBody,
            JsonSerializerConfiguration.CamelCase);
        
        if(response == null)
            throw new JsonException($"Deserializing response to '{nameof(GetDnsEntriesResponse)}' returned null value.");

        return response.DnsEntries.Select(dnsEntry => new Core.DnsEntry
        {
            Content = dnsEntry.Content,
            Name = dnsEntry.Name,
            ExpiresAt = dnsEntry.Expire,
            Type = dnsEntry.Type
        });
    }

    public async Task UpdateDnsEntriesAsync(string domain, Core.DnsEntry dnsEntry, string externalIp, CancellationToken cancellationToken)
    {
        var transIpDnsEntry = new Requests.DnsEntry
        {
            Content = dnsEntry.Content,
            Name = dnsEntry.Name,
            Expire = dnsEntry.ExpiresAt,
            Type = dnsEntry.Type
        };
        
        var requestBody = new UpdateDnsEntryRequestBody(transIpDnsEntry);
        
        var dnsEntriesRequest = new HttpRequestMessage(HttpMethod.Patch, 
            new Uri($"https://api.transip.nl/v6/domains/{domain}/dns")); 
        var body = JsonSerializer.Serialize(requestBody, JsonSerializerConfiguration.CamelCase); 
 
        dnsEntriesRequest.Content = new StringContent(body); 
 
        var response = await httpClient.SendAsync(dnsEntriesRequest, cancellationToken);
        if (response.IsSuccessStatusCode) return;
 
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken); 
        throw new DnsProviderException($"Failed to update DNS entry: {responseBody}"); 
    }
}