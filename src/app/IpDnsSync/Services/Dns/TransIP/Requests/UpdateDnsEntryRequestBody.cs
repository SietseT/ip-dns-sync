namespace IpDnsSync.Services.Dns.TransIP.Requests;

public record UpdateDnsEntryRequestBody
{
    public UpdateDnsEntryRequestBody(DnsEntry dnsEntry)
    {
        DnsEntry = dnsEntry;
    }
    public DnsEntry DnsEntry { get; private set; }
}