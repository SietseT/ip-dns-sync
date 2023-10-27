using PublicDnsUpdater.Providers.TransIP.Requests;

namespace PublicDnsUpdater.Providers.TransIP.Responses;

public class GetDnsEntriesResponse
{
    public required IEnumerable<DnsEntry> DnsEntries { get; set; }
}