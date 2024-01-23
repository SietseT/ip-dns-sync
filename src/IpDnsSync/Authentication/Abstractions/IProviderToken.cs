namespace IpDnsSync.Authentication.Abstractions;

internal interface IProviderToken
{
    DateTime ExpiresAt { get; init; }
}