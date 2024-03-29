using IpDnsSync.Authentication;
using IpDnsSync.Authentication.Abstractions;
using IpDnsSync.Configuration;
using IpDnsSync.Core.Abstractions;
using IpDnsSync.Http;
using IpDnsSync.Services.ExternalIP;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IpDnsSync.Services.Dns.TransIP;

namespace IpDnsSync.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IProviderTokenManager, ProviderTokenManager>();
        services.AddOptions<Settings>().Bind(configuration.GetSection("Settings"));

        services.AddSingleton<IExternalIpService, IpifyService>();
        services.AddHttpClient<IpifyService>().AddPolicyHandler(Policies.GetDefaultRetryPolicy());

        return services;
    }
    
    public static IServiceCollection ConfigureProviders(this IServiceCollection services, IConfiguration configuration)
    {
        var configuredProviders = configuration.GetSection("Providers").GetChildren().Select(child => child.Key).ToArray();
        if(configuredProviders.Length == 0)
            throw new InvalidOperationException("No providers are configured. Please configure at least one provider.");
        
        foreach (var provider in configuredProviders)
        {
            RegisterProviderServices(services, configuration, provider);
        }
        
        return services;
    }
    
    private static void RegisterProviderServices(IServiceCollection services, IConfiguration configuration, string providerKey)
    {
        switch(providerKey)
        {
            case "TransIP":
                services.ConfigureTransIp(configuration);
                break;
            default:
                throw new NotSupportedException($"Provider {providerKey} is not supported.");
        }
    }
}