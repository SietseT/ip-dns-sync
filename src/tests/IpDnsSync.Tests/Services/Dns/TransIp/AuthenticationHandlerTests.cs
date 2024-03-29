using System.Net;
using System.Security.Cryptography;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using IpDnsSync.Authentication;
using IpDnsSync.Authentication.Abstractions;
using IpDnsSync.Configuration;
using IpDnsSync.Services.Dns.TransIP;
using RichardSzalay.MockHttp;
using HttpRequestMessage = System.Net.Http.HttpRequestMessage;

namespace IpDnsSync.Tests.Services.Dns.TransIp;

public class AuthenticationHandlerTests
{
    private static readonly Fixture Fixture = new();
    
    [Fact]
    public async Task SendAsync_WhenTokenIsNotCached_ShouldRequestNewToken()
    {
        // Arrange
        var tokenManager = Substitute.For<IProviderTokenManager>();
        tokenManager.GetToken<ProviderJwt>(Provider.TransIp).Returns((ProviderJwt)null!);

        var configuration = Substitute.For<IOptions<TransIpConfiguration>>();
        var providerConfig = new TransIpConfiguration
        {
            Username = "test",
            PrivateKey = new RSACryptoServiceProvider().ExportRSAPrivateKeyPem()
        };
            
        configuration.Value.Returns(providerConfig);

        var token = Fixture.Create<string>();

        var originHttpClientMock = new MockHttpMessageHandler();
        var originRequest = originHttpClientMock.When("https://example.com")
            .WithHeaders("Authorization", $"Bearer {token}")
            .Respond(HttpStatusCode.OK);
        
        var sutHttpClientMock = new MockHttpMessageHandler();
        var getTokenRequest = sutHttpClientMock.When("https://api.transip.nl/v6/auth")
            .Respond("application/json", $"{{\"token\": \"{token}\"}}");
        var sut = new AuthenticationHandler(tokenManager, configuration, sutHttpClientMock.ToHttpClient())
        {
            InnerHandler = originHttpClientMock
        };

        var invoker = new HttpMessageInvoker(sut);

        // Act
        await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.com"), CancellationToken.None);

        // Assert
        await tokenManager.Received(1).GetToken<ProviderJwt>(Provider.TransIp);
        sutHttpClientMock.AddRequestExpectation(getTokenRequest);
        originHttpClientMock.AddRequestExpectation(originRequest);
    }
    
    [Fact]
    public async Task SendAsync_WhenTokenNotCached_ShouldUseTokenFromCache()
    {
        // Arrange
        var cachedToken = Fixture.Create<ProviderJwt>();
        var tokenManager = Substitute.For<IProviderTokenManager>();
        tokenManager.GetToken<ProviderJwt>(Provider.TransIp).Returns(cachedToken);

        var configuration = Substitute.For<IOptions<TransIpConfiguration>>();
        var providerConfig = Fixture.Create<TransIpConfiguration>();
        configuration.Value.Returns(providerConfig);

        var originHttpClientMock = new MockHttpMessageHandler();
        var originRequest = originHttpClientMock.When("https://example.com")
            .WithHeaders("Authorization", $"Bearer {cachedToken.Token}")
            .Respond(HttpStatusCode.OK);

        var sutHttpClientMock = Substitute.For<HttpClient>();
        var sut = new AuthenticationHandler(tokenManager, configuration, sutHttpClientMock)
        {
            InnerHandler = originHttpClientMock
        };

        var invoker = new HttpMessageInvoker(sut);

        // Act
        await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.com"), CancellationToken.None);

        // Assert
        originHttpClientMock.AddRequestExpectation(originRequest);
        await sutHttpClientMock.DidNotReceive().SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task SendAsync_WhenTokenIsNotCached_ThrowsExceptionWhenRetrievingNonSuccesfulResponse()
    {
        // Arrange
        var tokenManager = Substitute.For<IProviderTokenManager>();
        tokenManager.GetToken<ProviderJwt>(Provider.TransIp).Returns((ProviderJwt)null!);

        var configuration = Substitute.For<IOptions<TransIpConfiguration>>();
        var providerConfig = new TransIpConfiguration
        {
            Username = "test",
            PrivateKey = new RSACryptoServiceProvider().ExportRSAPrivateKeyPem()
        };
        configuration.Value.Returns(providerConfig);

        var originHttpClientMock = new MockHttpMessageHandler();
        originHttpClientMock.When("https://example.com")
            .WithHeaders("Authorization", $"Bearer {Fixture.Create<string>()}")
            .Respond(HttpStatusCode.OK);
        
        var sutHttpClientMock = new MockHttpMessageHandler();
        sutHttpClientMock.When("https://api.transip.nl/v6/auth")
            .Respond(HttpStatusCode.InternalServerError);
        
        var sut = new AuthenticationHandler(tokenManager, configuration, sutHttpClientMock.ToHttpClient())
        {
            InnerHandler = originHttpClientMock
        };

        var invoker = new HttpMessageInvoker(sut);

        // Act
        var sutAction = async() => await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://example.com"), CancellationToken.None);

        // Assert
        await sutAction.Should().ThrowAsync<Exception>();
    }
}