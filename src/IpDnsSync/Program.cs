using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using IpDnsSync.Core.Extensions;

var builder = Host.CreateApplicationBuilder();

builder.Configuration.AddConfiguration(new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile("appsettings.Development.json", true, true)
    .AddEnvironmentVariables()
    .Build());

builder.Services
    .ConfigureCore(builder.Configuration)
    .ConfigureProviders(builder.Configuration)
    .RemoveAll<IHttpMessageHandlerBuilderFilter>();

var host = builder.Build();
await host.StartAsync();