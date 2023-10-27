using PublicDnsUpdater.Authentication.Abstractions;
using PublicDnsUpdater.Configuration;

namespace PublicDnsUpdater.Authentication;

internal class ProviderTokenManager : IProviderTokenManager
{
    private readonly Dictionary<string, (IProviderToken Token, Func<Task<IProviderToken>> RefreshMethod)> _providerTokens = new();
    
    public void StoreToken(Provider provider, IProviderToken token, Func<Task<IProviderToken>> refreshToken)
    {
        if(token.ExpiresAt < DateTime.UtcNow) throw new ArgumentException("Token must not be expired", nameof(token));

        _providerTokens[provider] = (token, refreshToken);
    }

    public async Task<IProviderToken> GetToken(Provider provider)
    {
        var providerToken = _providerTokens.TryGetValue(provider, out var token) ? token : throw new KeyNotFoundException(provider);
        
        if(providerToken.Token.ExpiresAt > DateTime.UtcNow) return providerToken.Token;

        try
        {
            var newToken = await providerToken.RefreshMethod();
            StoreToken(provider, newToken, providerToken.RefreshMethod);
            return newToken;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to refresh token. See inner exception for details.", ex);
        }
    }
}