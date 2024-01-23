using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using IpDnsSync.Authentication;
using IpDnsSync.Authentication.Abstractions;
using IpDnsSync.Configuration;
using IpDnsSync.Helpers;
using IpDnsSync.Services.Dns.TransIP.Requests;
using IpDnsSync.Services.Dns.TransIP.Responses;
using Microsoft.Extensions.Options;

namespace IpDnsSync.Services.Dns.TransIP;

internal class AuthenticationHandler : DelegatingHandler
{
    private readonly TransIpConfiguration _transIpConfiguration;
    private readonly IProviderTokenManager _providerTokenManager;
    private readonly HttpClient _httpClient;

    public AuthenticationHandler(IProviderTokenManager providerTokenManager,
        IOptions<TransIpConfiguration> transIpConfiguration,
        HttpClient httpClient)
    {
        _providerTokenManager = providerTokenManager;
        _httpClient = httpClient;
        _transIpConfiguration = transIpConfiguration.Value;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _providerTokenManager.GetToken<ProviderJwt>(Provider.TransIp);
        if (token == null)
        {
            token = await GetToken(cancellationToken);
            _providerTokenManager.StoreToken(Provider.TransIp, token, () => GetToken(cancellationToken));
        }
        
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<ProviderJwt> GetToken(CancellationToken cancellationToken)
    {
        var timestamp = DateTime.Now.Ticks.ToString();
        
        var json = JsonSerializer.Serialize(new GetTokenBody
        {
            Login = _transIpConfiguration.Username,
            Label = $"public-dns-updater-{timestamp}",
            Nonce = Guid.NewGuid().ToString("N")[..12],
            ExpirationTime = "5 minutes",
            GlobalKey = true,
            ReadOnly = false
        }, JsonSerializerConfiguration.SnakeCase);
        
        var signature = JwtSignature.Sign(json, _transIpConfiguration.PrivateKey);
        var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://api.transip.nl/v6/auth"));
        request.Headers.Add("Signature", signature);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.SendAsync(request, cancellationToken);
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