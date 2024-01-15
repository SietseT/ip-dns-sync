using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PublicDnsUpdater.Authentication;
using PublicDnsUpdater.Authentication.Abstractions;
using PublicDnsUpdater.Configuration;
using PublicDnsUpdater.Helpers;
using PublicDnsUpdater.Providers.TransIP.Requests;
using PublicDnsUpdater.Providers.TransIP.Responses;

namespace PublicDnsUpdater.Providers.TransIP;

internal class AuthenticationHandler(IProviderTokenManager providerTokenManager,
    IOptions<ProviderConfiguration<TransIpConfiguration>> transIpConfiguration,
    IHttpClientFactory httpClientFactory)
    : DelegatingHandler
{
    private readonly ProviderConfiguration<TransIpConfiguration> _transIpConfiguration = transIpConfiguration.Value;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await providerTokenManager.GetToken<ProviderJwt>(Provider.TransIp);
        if (token == null)
        {
            token = await GetToken(cancellationToken);
            providerTokenManager.StoreToken(Provider.TransIp, token, () => GetToken(cancellationToken));
        }
        
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<ProviderJwt> GetToken(CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(new GetTokenBody
        {
            Login = _transIpConfiguration.Provider.Username,
            Label = "public-dns-updater",
            Nonce = Guid.NewGuid().ToString("N")[..12],
            ExpirationTime = "6 minutes",
            GlobalKey = true,
            ReadOnly = false
        }, JsonSerializerConfiguration.SnakeCase);

        var privateKey = new StringBuilder();
        privateKey.AppendLine("-----BEGIN PRIVATE KEY-----");
        privateKey.AppendLine(_transIpConfiguration.Provider.PrivateKey);
        privateKey.AppendLine("-----END PRIVATE KEY-----");
        
        var signature = JwtSignature.Sign(json, privateKey.ToString());
        var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://api.transip.nl/v6/auth"));
        request.Headers.Add("Signature", signature);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpClient = httpClientFactory.CreateClient();
        var response = await httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if(!response.IsSuccessStatusCode)
            throw new Exception($"Failed to get token: ${responseBody}");

        var token = JsonSerializer.Deserialize<GetTokenResponse>(responseBody, JsonSerializerConfiguration.SnakeCase);
        if(string.IsNullOrEmpty(token?.Token))
            throw new Exception($"Failed to deserialize response to {nameof(GetTokenResponse)}: ${responseBody}");

        return new ProviderJwt
        {
            ExpiresAt = DateTime.Now.AddMinutes(5),
            Token = token.Token 
        };
    }
}