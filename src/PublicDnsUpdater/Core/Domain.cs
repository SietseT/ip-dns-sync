namespace PublicDnsUpdater.Core;

public record Domain
{
    public required string Name { get; init; }
    public IEnumerable<DnsEntry> DnsEntries { get; init; } = new List<DnsEntry>();
}