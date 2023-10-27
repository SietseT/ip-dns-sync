using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PublicDnsUpdater.Configuration;
using PublicDnsUpdater.Http;

namespace PublicDnsUpdater.Providers.TransIP;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureTransIp(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<AuthenticationHandler>();
        services.AddHttpClient(Configuration.Provider.TransIp)
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(Policies.GetDefaultRetryPolicy())
            .AddHttpMessageHandler<AuthenticationHandler>();
        
        services.Configure<ProviderConfiguration<TransIpConfiguration>>(configuration.GetSection("TransIP"));
        services.AddOptions<TransIpConfiguration>();

        return services;
    }
}