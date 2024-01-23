using IpDnsSync.Configuration;

namespace IpDnsSync.Services.Dns.TransIP;

public record TransIpConfiguration : ProviderConfigurationBase
{
    public string Username { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
}