using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IpDnsSync.Configuration;
using IpDnsSync.Core.Abstractions;
using IpDnsSync.Core.Extensions;
using IpDnsSync.Services.Dns.TransIP;

namespace IpDnsSync.Tests.Core.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void ConfigureProviders_WithConfiguredProvider_ShouldRegisterProviderServices()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Providers:TransIP:Username", "TestUser"}
            })
            .Build();
        
        var services = new ServiceCollection();
        services.ConfigureCore(configuration);
        services.ConfigureProviders(configuration);
        
        // Act
        var provider = services.BuildServiceProvider();
        
        // Assert
        var dnsProviderService = provider.GetRequiredService<TransIpService>();
        dnsProviderService.Should().NotBeNull();
        var workers = provider.GetServices<IHostedService>();
        workers.Should().Contain(x => x is IUpdateDnsWorker && ((IUpdateDnsWorker) x).Provider == Provider.TransIp);
    }
    
    [Fact]
    public void ConfigureProviders_WithoutConfiguredProviders_ShouldThrowException()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Providers:NonExistingDnsProvider:Username", "TestUser"}
            })
            .Build();
        
        var services = new ServiceCollection();
        services.ConfigureCore(configuration);
        
        // Act
        var sut = () => services.ConfigureProviders(configuration);
        
        // Assert
        sut.Should().Throw<NotSupportedException>().WithMessage("*not supported*");
    }
    
    [Fact]
    public void ConfigureProviders_UsingInvalidProvider_ShouldThrowException()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();
        
        var services = new ServiceCollection();
        services.ConfigureCore(configuration);
        
        // Act
        var sut = () => services.ConfigureProviders(configuration);
        
        // Assert
        sut.Should().Throw<InvalidOperationException>().WithMessage("*configure at least one provider*");
    }
}