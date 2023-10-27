using Microsoft.Extensions.DependencyInjection;
using PublicDnsUpdater.Configuration;
using PublicDnsUpdater.Http;

namespace PublicDnsUpdater.Providers.TransIP;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureTransIp(this IServiceCollection services)
    {
        services.AddTransient<AuthenticationHandler>();
        services.AddHttpClient(Configuration.Provider.TransIp)
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(Policies.GetDefaultRetryPolicy())
            .AddHttpMessageHandler<AuthenticationHandler>();

        return services;
    }
}