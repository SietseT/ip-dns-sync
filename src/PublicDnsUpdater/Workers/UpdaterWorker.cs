using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PublicDnsUpdater.Configuration;
using PublicDnsUpdater.Helpers;
using PublicDnsUpdater.Http;
using PublicDnsUpdater.Providers.TransIP;

namespace PublicDnsUpdater.Workers;

public class UpdaterWorker : IHostedService, IDisposable
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ProviderConfiguration<TransIpConfiguration> _transIpConfiguration;

    public UpdaterWorker(IHttpClientFactory httpClientFactory, IOptions<ProviderConfiguration<TransIpConfiguration>> transIpConfiguration)
    {
        _httpClientFactory = httpClientFactory;
        _transIpConfiguration = transIpConfiguration.Value;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient(Provider.TransIp);

        var json = JsonSerializer.Serialize(new GetTokenBody
        {
            Login = _transIpConfiguration.Provider.Username,
            Label = "public-dns-updater",
            Nonce = Guid.NewGuid().ToString("N")[..12],
            ExpirationTime = "5 minutes",
            GlobalKey = true,
            ReadOnly = false
        }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance
        });

        var privateKey = new StringBuilder();
        privateKey.AppendLine("-----BEGIN PRIVATE KEY-----");
        privateKey.AppendLine(_transIpConfiguration.Provider.PrivateKey);
        privateKey.AppendLine("-----END PRIVATE KEY-----");
        
        var signature = JwtSignature.Sign(json, privateKey.ToString());

        var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://api.transip.nl/v6/auth"));
        request.Headers.Add("Signature", signature);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}