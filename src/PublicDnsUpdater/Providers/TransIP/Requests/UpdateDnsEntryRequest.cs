namespace PublicDnsUpdater.Providers.TransIP.Requests;

public record UpdateDnsEntryRequest
{
    public required DnsEntry DnsEntry { get; init; }
}