namespace PublicDnsUpdater.Core.Abstractions;

public interface IDnsProviderService
{
    Task<IEnumerable<DnsEntry>> GetDnsEntriesForDomainAsync(string domain, CancellationToken cancellationToken);

    Task UpdateDnsEntriesAsync(string domain, DnsEntry dnsEntry, string externalIp, CancellationToken cancellationToken);
}