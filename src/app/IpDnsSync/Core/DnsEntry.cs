namespace IpDnsSync.Core;

public record DnsEntry
{
    public required string Name { get; init; }
    public required long ExpiresAt { get; init; }
    public required string Type { get; init; }
    public required string Content { get; set; }
}