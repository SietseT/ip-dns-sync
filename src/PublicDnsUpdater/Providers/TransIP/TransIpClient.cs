using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PublicDnsUpdater.Configuration;
using PublicDnsUpdater.Core;
using PublicDnsUpdater.Helpers;
using PublicDnsUpdater.Providers.TransIP.Abstractions;
using PublicDnsUpdater.Providers.TransIP.Requests;
using PublicDnsUpdater.Providers.TransIP.Responses;

namespace PublicDnsUpdater.Providers.TransIP;

public class TransIpClient(
    IHttpClientFactory httpClientFactory,
    ILogger<TransIpClient> logger)
    : ITransIpClient
{

    public async Task<HttpResponse<GetTokenResponse>> GetTokenAsync(GetTokenBody body, string jwtSignature, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(body, JsonSerializerConfiguration.SnakeCase);
        var httpClient = httpClientFactory.CreateClient();
        
        var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://api.transip.nl/v6/auth"));
        request.Headers.Add("Signature", jwtSignature);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if(!response.IsSuccessStatusCode)
            throw new Exception($"Failed to get token: ${responseBody}");

        try
        {
            var token = JsonSerializer.Deserialize<GetTokenResponse>(responseBody, JsonSerializerConfiguration.SnakeCase);

            if (token == null)
            {
                throw new JsonException($"Failed to deserialize response to '{nameof(GetTokenResponse)}'.");
            }
            if (string.IsNullOrEmpty(token.Token))
            {
                throw new JsonException($"Return response did not contain a valid 'Token' parameter.");
            }

            return new HttpResponse<GetTokenResponse>(token);
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to deserialize response: {Message}. Response was: '{Body}'", ex.Message, responseBody);
            return new HttpResponse<GetTokenResponse>(ex);
        }
    }

    public async Task<HttpResponse<GetDnsEntriesResponse>> GetDnsEntriesAsync(string domain, CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient(Provider.TransIp);

        try
        {
            var dnsEntriesRequest = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://api.transip.nl/v6/domains/{domain}/dns"));
            var dnsEntriesResponse = await httpClient.SendAsync(dnsEntriesRequest, cancellationToken);
            var dnsEntriesResponseBody = await dnsEntriesResponse.Content.ReadAsStringAsync(cancellationToken);
            var getDnsEntries = JsonSerializer.Deserialize<GetDnsEntriesResponse>(dnsEntriesResponseBody,
                JsonSerializerConfiguration.CamelCase);
            
            if(getDnsEntries == null)
                throw new JsonException($"Deserializing response to '{nameof(GetDnsEntriesResponse)}' returned null.");

            return new HttpResponse<GetDnsEntriesResponse>(getDnsEntries);

        }
        catch (Exception ex)
        {
            logger.LogError("Error while deserializing response to '{Type}': {Message}", nameof(GetDnsEntriesResponse), ex.Message);
            return new HttpResponse<GetDnsEntriesResponse>(ex);
        }
    }

    public async Task<HttpResponse<UpdateDnsEntryResponse>> UpdateDnsEntryAsync(string domain, UpdateDnsEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient(Provider.TransIp);

        try
        {

            var dnsEntriesRequest = new HttpRequestMessage(HttpMethod.Patch,
                new Uri($"https://api.transip.nl/v6/domains/{domain}/dns"));
            var body = JsonSerializer.Serialize(request, JsonSerializerConfiguration.CamelCase);

            dnsEntriesRequest.Content = new StringContent(body);

            var response = await httpClient.SendAsync(dnsEntriesRequest, cancellationToken);
            if (response.IsSuccessStatusCode) return new HttpResponse<UpdateDnsEntryResponse>(new UpdateDnsEntryResponse());

            var responseBody = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to update DNS entry: {responseBody}");
        }
        catch (Exception ex)
        {
            return new HttpResponse<UpdateDnsEntryResponse>(ex);
        }
    }
}