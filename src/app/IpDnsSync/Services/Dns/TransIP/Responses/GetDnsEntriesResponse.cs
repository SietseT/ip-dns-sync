using IpDnsSync.Services.Dns.TransIP.Requests;

namespace IpDnsSync.Services.Dns.TransIP.Responses;

public record GetDnsEntriesResponse
{
    public required IEnumerable<DnsEntry> DnsEntries { get; init; }
}