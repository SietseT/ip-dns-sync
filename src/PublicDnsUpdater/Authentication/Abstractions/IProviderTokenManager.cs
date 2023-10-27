using PublicDnsUpdater.Configuration;

namespace PublicDnsUpdater.Authentication.Abstractions;

internal interface IProviderTokenManager
{
    void StoreToken(Provider provider, IProviderToken token, Func<Task<IProviderToken>> refreshToken);
    Task<IProviderToken> GetToken(Provider provider);
}