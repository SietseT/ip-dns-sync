namespace IpDnsSync.Configuration;

public abstract record ProviderConfigurationBase
{
    public IEnumerable<string> Domains { get; init; } = Array.Empty<string>();
}