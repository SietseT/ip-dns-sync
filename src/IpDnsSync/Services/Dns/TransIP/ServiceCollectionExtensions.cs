using IpDnsSync.Configuration;
using IpDnsSync.Core;
using IpDnsSync.Core.Abstractions;
using IpDnsSync.Http;
using IpDnsSync.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IpDnsSync.Services.Dns.TransIP;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureTransIp(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<AuthenticationHandler>();
        services.AddTransient<TransIpService>();
        services.AddHttpClient<TransIpService>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(Policies.GetDefaultRetryPolicy())
            .AddHttpMessageHandler<AuthenticationHandler>();
        
        services.Configure<TransIpConfiguration>(configuration.GetSection("Providers:TransIP"));
        services.AddOptions<TransIpConfiguration>();
        
        services.AddHostedService<IUpdateDnsWorker>(builder =>
        {
            var loggerFactory = builder.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger($"{Provider.TransIp}Worker");
            var settings = builder.GetRequiredService<IOptions<Settings>>().Value;
            
            var updateDnsService = new UpdateDnsService(
                builder.GetRequiredService<TransIpService>(),
                builder.GetRequiredService<IExternalIpService>(),
                settings,
                builder.GetRequiredService<IOptions<TransIpConfiguration>>().Value.Domains,
                logger
            );
            
            return new UpdateDnsWorker(updateDnsService, Provider.TransIp, settings, logger);
        });

        return services;
    }
}