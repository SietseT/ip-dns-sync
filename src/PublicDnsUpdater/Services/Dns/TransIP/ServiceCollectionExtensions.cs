using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PublicDnsUpdater.Configuration;
using PublicDnsUpdater.Core;
using PublicDnsUpdater.Core.Abstractions;
using PublicDnsUpdater.Http;
using PublicDnsUpdater.Workers;

namespace PublicDnsUpdater.Services.Dns.TransIP;

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