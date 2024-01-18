// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PublicDnsUpdater.Core.Extensions;

var builder = Host.CreateApplicationBuilder();

builder.Configuration.AddConfiguration(new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile("appsettings.Development.json", true, true)
    .Build());

builder.Services
    .ConfigureCore(builder.Configuration)
    .ConfigureProviders(builder.Configuration);

var host = builder.Build();
await host.StartAsync();