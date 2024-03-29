using IpDnsSync.Configuration;
using IpDnsSync.Core.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IpDnsSync.Core;

public class UpdateDnsService : IUpdateDnsService
{
    private readonly IDnsProviderService _service;
    private readonly IExternalIpService _externalIpService;
    private readonly Settings _settings;
    private readonly IEnumerable<string> _domains;
    private readonly ILogger _logger;

    public UpdateDnsService(IDnsProviderService service,
        IExternalIpService externalIpService,
        Settings settings,
        IEnumerable<string> domains,
        ILogger logger)
    {
        _service = service;
        _externalIpService = externalIpService;
        _settings = settings;
        _domains = domains;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var externalIp = _settings.TestMode
            ? _settings.TestModeIpAddress
            : await _externalIpService.GetExternalIpAsync();

        if (string.IsNullOrEmpty(externalIp))
        {
            _logger.LogError("Did not receive an external IP address");
            return;
        }
        
        foreach (var domain in _domains)
        {
            try
            {
                await UpdateDnsEntriesForDomain(domain, externalIp, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update DNS entry for {Domain}", domain);
            }
        }
    }

    private async Task UpdateDnsEntriesForDomain(string domain, string externalIp, CancellationToken cancellationToken)
    {
        var dnsEntries = await GetDnsEntriesToUpdate(domain, externalIp, cancellationToken);
        if (dnsEntries.Length == 0)
        {
            _logger.LogInformation("No DNS entries to update for {Domain}", domain);
        }
        else
        {
            foreach (var dnsEntry in dnsEntries)
            {
                await _service.UpdateDnsEntriesAsync(domain, dnsEntry, externalIp, cancellationToken);
                _logger.LogInformation("Updated DNS entry {DnsEntry} for {Domain} to {ExternalIp}", dnsEntry.Name, domain, externalIp);
            }
        }
    }
    
    private async Task<DnsEntry[]> GetDnsEntriesToUpdate(string domain, string externalIp, CancellationToken cancellationToken)
    {
        // Check if domain has subdomain
        var domainParts = domain.Split('.').Where(part => !string.IsNullOrWhiteSpace(part)).ToArray();
        
        bool isSubDomain;
        var rootDomain = domain;

        if (domainParts is { Length: 2 })
            isSubDomain = false;
        else if (domainParts is { Length: 3 })
        {
            isSubDomain = true;
            rootDomain = $"{domainParts[1]}.{domainParts[2]}";
        }
        else
            throw new FormatException($"Invalid domain format: ${domain}. A domain should be in the format 'example.com' or 'sub.example.com'.");
        
        var dnsEntries = (await _service.GetDnsEntriesForDomainAsync(rootDomain, cancellationToken)).ToArray();
        
        dnsEntries = dnsEntries.Where(x => x.Content != externalIp && x.Type == "A").ToArray();
        
        if (!isSubDomain)
            return dnsEntries.Where(dnsEntry => dnsEntry.Name is "@" or "www").ToArray();
        
        return dnsEntries.Where(dnsEntry => dnsEntry.Name == domainParts[0]).ToArray();
    }
}