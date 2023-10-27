using AutoFixture.Xunit2;
using FluentAssertions;
using PublicDnsUpdater.Authentication;
using PublicDnsUpdater.Authentication.Abstractions;
using PublicDnsUpdater.Configuration;

namespace PublicDnsUpdater.Tests;

public class ProviderTokenManagerTests
{
    [Theory, AutoData]
    public async Task GetToken_ForToken_ReturnsToken(string token)
    {
        // Arrange
        var jwt = new ProviderJwt
        {
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            Token = token
        };
        
        var tokenManager = new ProviderTokenManager();
        tokenManager.StoreToken(Provider.TransIp, jwt, () => Task.FromResult<IProviderToken>(jwt));

        // Act
        var result = await tokenManager.GetToken(Provider.TransIp);

        // Assert
        result.Should().Be(jwt);
    }
    
    [Theory, AutoData]
    public async Task GetToken_ForExpiredToken_RefreshesAndReturnsToken(string expiredToken, string newToken)
    {
        // Arrange
        var expiredJwt = new ProviderJwt
        {
            ExpiresAt = DateTime.UtcNow.AddMilliseconds(50),
            Token = expiredToken
        };
        
        var newJwt = new ProviderJwt
        {
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            Token = newToken
        };

        var tokenManager = new ProviderTokenManager();
        tokenManager.StoreToken(Provider.TransIp, expiredJwt, RefreshToken);

        // Act
        await Task.Delay(100); // Expire the token
        var result = await tokenManager.GetToken(Provider.TransIp);

        // Assert
        result.Should().Be(newJwt);
        return;

        Task<IProviderToken> RefreshToken() => Task.FromResult<IProviderToken>(newJwt);
    }
    
    [Theory, AutoData]
    public async Task GetToken_ForExpiredToken_RefreshesTokenAndThrows(string token)
    {
        // Arrange
        var jwt = new ProviderJwt
        {
            ExpiresAt = DateTime.UtcNow.AddMilliseconds(50),
            Token = token
        };
        
        var tokenManager = new ProviderTokenManager();
        var exception = new TimeoutException();
        tokenManager.StoreToken(Provider.TransIp, jwt, () => throw exception);

        // Act
        await Task.Delay(100); // Expire the token
        var act = async () => await tokenManager.GetToken(Provider.TransIp);

        // Assert
        await act.Should().ThrowAsync<Exception>().Where(e => e.InnerException == exception);
    }
} 