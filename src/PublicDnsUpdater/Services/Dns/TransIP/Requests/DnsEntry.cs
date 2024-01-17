namespace PublicDnsUpdater.Services.Dns.TransIP.Requests;

public record DnsEntry
{
    public required string Name { get; init; }
    public required long Expire { get; init; }
    public required string Type { get; init; }
    public required string Content { get; set; }
}