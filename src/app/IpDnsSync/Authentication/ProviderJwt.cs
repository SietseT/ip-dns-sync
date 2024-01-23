using IpDnsSync.Authentication.Abstractions;

namespace IpDnsSync.Authentication;

internal class ProviderJwt : IProviderToken
{
    public required DateTime ExpiresAt { get; init; }
    public required string Token { get; init; }
}