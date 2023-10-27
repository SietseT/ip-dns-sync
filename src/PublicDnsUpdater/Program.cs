// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PublicDnsUpdater.Authentication;
using PublicDnsUpdater.Authentication.Abstractions;
using PublicDnsUpdater.Configuration;
using PublicDnsUpdater.Providers.TransIP;
using PublicDnsUpdater.Workers;

var builder = Host.CreateApplicationBuilder();

builder.Configuration.AddConfiguration(new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile("appsettings.Development.json", true, true)
    .Build());

builder.Services.AddHostedService<UpdaterWorker>();
builder.Services.AddSingleton<IProviderTokenManager, ProviderTokenManager>();

builder.Services.ConfigureTransIp(builder.Configuration);



var host = builder.Build();
await host.StartAsync();