using Microsoft.Extensions.Logging;
using NSubstitute;
using PublicDnsUpdater.Core;
using PublicDnsUpdater.Core.Abstractions;
using PublicDnsUpdater.Tests.Extensions;

namespace PublicDnsUpdater.Tests.Core;

public class UpdateDnsServiceTests
{
    [Fact]
    public async Task Execute_ForRoot_UpdatesRootDnsEntries()
    {
        // Arrange
        const string domain = "example.com";
        var dnsEntries = new[]
        {
            new DnsEntry
            {
                Name = "www",
                Type = "A",
                Content = "1.1.1.1",
                ExpiresAt = 0
            },
            new DnsEntry
            {
                Name = "@",
                Type = "A",
                Content = "1.1.1.1",
                ExpiresAt = 0
            },
            new DnsEntry
            {
                Name = "sub",
                Type = "A",
                Content = "1.1.1.1",
                ExpiresAt = 0
            }
        };

        const string externalIp = "2.2.2.2";
        
        var service = Substitute.For<IDnsProviderService>();
        service.GetDnsEntriesForDomainAsync(domain, Arg.Any<CancellationToken>()).Returns(Task.FromResult(dnsEntries.AsEnumerable()));
        
        var externalIpProvider = Substitute.For<IExternalIpService>();
        externalIpProvider.GetExternalIpAsync()!.Returns(Task.FromResult(externalIp));
        
        var cancellationToken = CancellationToken.None;
        var logger = Substitute.For<ILogger<UpdateDnsService>>();
        
        var sut = new UpdateDnsService(service, externalIpProvider, new[] { domain }, logger);

        // Act
        await sut.ExecuteAsync(cancellationToken);

        // Assert
        var expectedDnsEntries = dnsEntries.Where(dnsEntry => dnsEntry.Name is "@" or "www").ToArray();
        foreach (var expectedDnsEntry in expectedDnsEntries)
        {
            await service.Received(1).UpdateDnsEntriesAsync(domain, expectedDnsEntry, externalIp, cancellationToken);
        }
        
        logger.Received(expectedDnsEntries.Length).AnyLogOfType(LogLevel.Information);
    }
    
    
    [Fact]
    public async Task Execute_ForSubdomain_UpdatesSubdomainDnsEntry()
    {
        // Arrange
        const string domain = "sub.example.com";
        var dnsEntries = new[]
        {
            new DnsEntry
            {
                Name = "@",
                Type = "A",
                Content = "1.1.1.1",
                ExpiresAt = 0
            },
            new DnsEntry
            {
                Name = "sub",
                Type = "A",
                Content = "1.1.1.1",
                ExpiresAt = 0
            }
        };

        const string externalIp = "2.2.2.2";
        
        var service = Substitute.For<IDnsProviderService>();
        service.GetDnsEntriesForDomainAsync(domain, Arg.Any<CancellationToken>()).Returns(Task.FromResult(dnsEntries.AsEnumerable()));
        
        var externalIpProvider = Substitute.For<IExternalIpService>();
        externalIpProvider.GetExternalIpAsync()!.Returns(Task.FromResult(externalIp));
        
        var cancellationToken = CancellationToken.None;
        var logger = Substitute.For<ILogger<UpdateDnsService>>();
        
        var sut = new UpdateDnsService(service, externalIpProvider, new[] { domain }, logger);

        // Act
        await sut.ExecuteAsync(cancellationToken);

        // Assert
        await service.Received(1).UpdateDnsEntriesAsync(domain, dnsEntries.First(e => e.Name is "sub"), externalIp, cancellationToken);
        logger.Received(1).AnyLogOfType(LogLevel.Information);
    }
    
    [Fact]
    public async Task Execute_UnchangedIp_SkipsUpdate()
    {
        // Arrange
        const string domain = "example.com";
        var dnsEntries = new[]
        {
            new DnsEntry
            {
                Name = "@",
                Type = "A",
                Content = "1.1.1.1",
                ExpiresAt = 0
            },
            new DnsEntry
            {
                Name = "www",
                Type = "A",
                Content = "2.2.2.2",
                ExpiresAt = 0
            }
        };

        const string externalIp = "1.1.1.1";
        
        var service = Substitute.For<IDnsProviderService>();
        service.GetDnsEntriesForDomainAsync(domain, Arg.Any<CancellationToken>()).Returns(Task.FromResult(dnsEntries.AsEnumerable()));
        
        var externalIpProvider = Substitute.For<IExternalIpService>();
        externalIpProvider.GetExternalIpAsync()!.Returns(Task.FromResult(externalIp));
        
        var cancellationToken = CancellationToken.None;
        var logger = Substitute.For<ILogger<UpdateDnsService>>();
        
        var sut = new UpdateDnsService(service, externalIpProvider, new[] { domain }, logger);

        // Act
        await sut.ExecuteAsync(cancellationToken);

        // Assert
        await service.DidNotReceive().UpdateDnsEntriesAsync(domain, Arg.Is(dnsEntries.First(e => e.Name is "@")), externalIp, cancellationToken);
        await service.Received(1).UpdateDnsEntriesAsync(domain, Arg.Is(dnsEntries.First(e => e.Name is "www")), externalIp, cancellationToken);
    }
    
    [Fact]
    public async Task Execute_ExternalIpProviderError_LogsError()
    {
        // Arrange
        var service = Substitute.For<IDnsProviderService>();
        
        var externalIpProvider = Substitute.For<IExternalIpService>();
        externalIpProvider.GetExternalIpAsync().Returns(Task.FromResult<string?>(null));
        
        var cancellationToken = CancellationToken.None;
        var logger = Substitute.For<ILogger<UpdateDnsService>>();
        
        var sut = new UpdateDnsService(service, externalIpProvider, new[] {"example.com" }, logger);

        // Act
        await sut.ExecuteAsync(cancellationToken);

        // Assert
        logger.Received(1).AnyLogOfType(LogLevel.Error);
        await service.DidNotReceive().GetDnsEntriesForDomainAsync(Arg.Any<string>(), cancellationToken);
    }
}