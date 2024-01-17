using PublicDnsUpdater.Services.Dns.TransIP.Requests;

namespace PublicDnsUpdater.Services.Dns.TransIP.Responses;

public record GetDnsEntriesResponse
{
    public required IEnumerable<DnsEntry> DnsEntries { get; init; }
}