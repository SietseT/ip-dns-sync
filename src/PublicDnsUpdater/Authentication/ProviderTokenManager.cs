using PublicDnsUpdater.Authentication.Abstractions;
using PublicDnsUpdater.Configuration;

namespace PublicDnsUpdater.Authentication;

internal class ProviderTokenManager : IProviderTokenManager
{
    private readonly Dictionary<string, (IProviderToken Token, object RefreshMethod)> _providerTokens = new();
    
    public void StoreToken<T>(Provider provider, IProviderToken token, Func<Task<T>> refreshToken) where T : class, IProviderToken
    {
        if(token.ExpiresAt < DateTime.UtcNow) throw new ArgumentException("Token must not be expired", nameof(token));

        _providerTokens[provider] = (token, refreshToken);
    }

    public async Task<T?> GetToken<T>(Provider provider) where T : class, IProviderToken
    {
        if(!_providerTokens.ContainsKey(provider)) return null;

        var providerToken = _providerTokens[provider];
        if(providerToken.Token.ExpiresAt > DateTime.UtcNow) return providerToken.Token as T ?? throw new Exception($"Token of type {providerToken.GetType()} is not of the expected type {typeof(T)}");

        try
        {
            var typedRefreshMethod = (Func<Task<T>>)providerToken.RefreshMethod;
            var newToken = await typedRefreshMethod();
            StoreToken(provider, newToken, typedRefreshMethod);
            
            return newToken;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to refresh token. See inner exception for details.", ex);
        }
    }
}