using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PublicDnsUpdater.Configuration;
using PublicDnsUpdater.Http;
using PublicDnsUpdater.Providers.TransIP.Abstractions;

namespace PublicDnsUpdater.Providers.TransIP;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureTransIp(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<AuthenticationHandler>();
        services.AddHttpClient(Provider.TransIp)
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(Policies.GetDefaultRetryPolicy())
            .AddHttpMessageHandler<AuthenticationHandler>();

        services.AddTransient<ITransIpClient, TransIpClient>();
        
        services.Configure<ProviderConfiguration<TransIpConfiguration>>(configuration.GetSection("TransIP"));
        services.AddOptions<TransIpConfiguration>();

        return services;
    }
}