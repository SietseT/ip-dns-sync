using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using IpDnsSync.Configuration;
using IpDnsSync.Core.Abstractions;
using IpDnsSync.Workers;

namespace IpDnsSync.Tests.Workers;

public class UpdateDnsWorkerTests
{
    [Fact]
    public async Task StartAsync_UsingDefaultSettings_ShouldExecuteOnce()
    {
        // Arrange
        var settings = new Settings();
        var service = Substitute.For<IUpdateDnsService>();
        var provider = Provider.TransIp;
        var logger = Substitute.For<ILogger>();
        var cancellationToken = new CancellationTokenSource();

        var sut = new UpdateDnsWorker(service, provider, settings, logger);

        // Act
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        sut.StartAsync(cancellationToken.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Thread.Sleep(100);
        await cancellationToken.CancelAsync();

        // Assert
        await service.Received(1).ExecuteAsync(Arg.Is(cancellationToken.Token));
    }
    
    [Fact]
    public async Task StartAsync_WithCustomInterval_ShouldExecuteMultipleTimes()
    {
        // Arrange
        var hundredMilliseconds = 0.001667;
        var settings = new Settings
        {
            UpdateIntervalInMinutes = hundredMilliseconds
        };
        var service = Substitute.For<IUpdateDnsService>();
        var provider = Provider.TransIp;
        var logger = Substitute.For<ILogger>();
        var cancellationToken = new CancellationTokenSource();

        var sut = new UpdateDnsWorker(service, provider, settings, logger);

        // Act
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        sut.StartAsync(cancellationToken.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Thread.Sleep(250);
        await cancellationToken.CancelAsync();

        // Assert
        await service.Received().ExecuteAsync(Arg.Is(cancellationToken.Token)); // Initial + 2 times
    }
}