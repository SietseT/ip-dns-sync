using Microsoft.Extensions.Logging;
using PublicDnsUpdater.Core.Abstractions;

namespace PublicDnsUpdater.Core;

public class UpdateDnsService(
    IDnsProviderService service,
    IExternalIpProvider externalIpProvider,
    IEnumerable<string> domains,
    ILogger<UpdateDnsService> logger) : IUpdateDnsService
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var externalIp = await externalIpProvider.GetExternalIpAsync();

        if (string.IsNullOrEmpty(externalIp))
        {
            logger.LogError("Did not receive an external IP address.");
            return;
        }
        
        foreach (var domain in domains)
        {
            try
            {
                await UpdateDnsEntriesForDomain(domain, externalIp, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update DNS entry for {domain}", domain);
            }
        }
    }

    private async Task UpdateDnsEntriesForDomain(string domain, string externalIp, CancellationToken cancellationToken)
    {
        var dnsEntries = await GetDnsEntriesToUpdate(domain, externalIp, cancellationToken);
        if (dnsEntries.Length == 0)
        {
            logger.LogInformation("No DNS entries to update for {domain}", domain);
        }
        else
        {
            foreach (var dnsEntry in dnsEntries)
            {
                await service.UpdateDnsEntriesAsync(domain, dnsEntry, externalIp, cancellationToken);
                logger.LogInformation("Updated DNS entry {dnsEntry} for {domain} to {externalIp}", dnsEntry.Name, domain, externalIp);
            }
        }
    }
    
    private async Task<DnsEntry[]> GetDnsEntriesToUpdate(string domain, string externalIp, CancellationToken cancellationToken)
    {
        //todo Make this configurable via DI
        var dnsEntries = (await service.GetDnsEntriesForDomainAsync(domain, cancellationToken)).ToArray();
        dnsEntries = dnsEntries.Where(x => x.Content != externalIp && x.Type == "A").ToArray();
        
        // Check if domain has subdomain
        var domainParts = domain.Split('.');

        if (domainParts is { Length: 2 })
            return dnsEntries.Where(dnsEntry => dnsEntry.Name is "@" or "www").ToArray();
        
        if (domainParts is { Length: > 2 })
            return dnsEntries.Where(dnsEntry => dnsEntry.Name == domainParts[0]).ToArray();

        throw new FormatException($"Invalid domain format: ${domain}. A domain should be in the format 'example.com' or 'sub.example.com'.");
    }
}