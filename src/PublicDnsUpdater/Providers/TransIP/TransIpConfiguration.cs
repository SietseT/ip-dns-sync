using PublicDnsUpdater.Configuration;

namespace PublicDnsUpdater.Providers.TransIP;

public record TransIpConfiguration
{
    public string Username { get; init; } = string.Empty;
    public string PrivateKey { get; init; } = string.Empty;
}