using Microsoft.Extensions.Logging;
using PublicDnsUpdater.Core.Abstractions;

namespace PublicDnsUpdater.Services.ExternalIP;

public class IpifyService(
    HttpClient httpClient, 
    ILogger<IpifyService> logger) : IExternalIpService
{
    public async Task<string?> GetExternalIpAsync()
    {
        try
        {
            var ip = await httpClient.GetStringAsync("https://api.ipify.org");
            return ip;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling Ipify: {Message}", ex.Message);
            return null;
        }
    }
}