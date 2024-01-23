using IpDnsSync.Configuration;

namespace IpDnsSync.Authentication.Abstractions;

internal interface IProviderTokenManager
{
    void StoreToken<T>(Provider provider, IProviderToken token, Func<Task<T>> refreshToken)
        where T : class, IProviderToken;
    Task<T?> GetToken<T>(Provider provider) where T : class, IProviderToken;
}