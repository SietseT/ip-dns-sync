using PublicDnsUpdater.Providers.TransIP.Requests;

namespace PublicDnsUpdater.Providers.TransIP.Responses;

public record GetDnsEntriesResponse
{
    public required IEnumerable<DnsEntry> DnsEntries { get; init; }
}