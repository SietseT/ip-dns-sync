namespace PublicDnsUpdater.Services.Dns.TransIP;

public record TransIpConfiguration
{
    public string Username { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
}