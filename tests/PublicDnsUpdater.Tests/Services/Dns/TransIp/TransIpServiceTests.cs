using System.Net;
using System.Text.Json;
using AutoFixture;
using FluentAssertions;
using PublicDnsUpdater.Core;
using PublicDnsUpdater.Services.Dns.TransIP;
using PublicDnsUpdater.Services.Dns.TransIP.Requests;
using RichardSzalay.MockHttp;
using DnsEntry = PublicDnsUpdater.Core.DnsEntry;

namespace PublicDnsUpdater.Tests.Services.Dns.TransIp;

public class TransIpServiceTests
{
    private static readonly Fixture Fixture = new();
    private static readonly CancellationToken CancellationToken = new();

    [Fact]
    public async Task GetDnsEntriesForDomainAsync_ForDomainWithEntries_ShouldReturnMappedEntries()
    {
        // Arrange
        var domain = Fixture.Create<string>();
        var httpClientMock = new MockHttpMessageHandler();
        httpClientMock.When("https://api.transip.nl/v6/domains/*/dns")
            .Respond("application/json", "{ \"dnsEntries\": [ { \"name\": \"@\", \"expire\": 123, \"type\": \"A\", \"content\": \"1.1.1.1\" } ] }");
        
        var sut = new TransIpService(httpClientMock.ToHttpClient());

        // Act
        var result = await sut.GetDnsEntriesForDomainAsync(domain, CancellationToken);

        // Assert
        result.Should().BeEquivalentTo(new DnsEntry[]
        {
            new () 
            {
                Content = "1.1.1.1",
                ExpiresAt = 123,
                Name = "@",
                Type = "A"
            }
        });
    }
    
    [Fact]
    public async Task GetDnsEntriesForDomainAsync_ForDomainWithoutEntries_ShouldReturnEmptyCollection()
    {
        // Arrange
        var domain = Fixture.Create<string>();
        var httpClientMock = new MockHttpMessageHandler();
        httpClientMock.When("https://api.transip.nl/v6/domains/*/dns")
            .Respond("application/json", "{ \"dnsEntries\": [] }");
        
        var sut = new TransIpService(httpClientMock.ToHttpClient());

        // Act
        var result = await sut.GetDnsEntriesForDomainAsync(domain, CancellationToken);

        // Assert
        result.Should().BeEquivalentTo(Array.Empty<DnsEntry>());
    }
    
    [Fact]
    public async Task GetDnsEntriesForDomainAsync_ForInvalidDomain_ShouldThrowExceptionContainingResponseBody()
    {
        // Arrange
        var domain = Fixture.Create<string>();
        var httpClientMock = new MockHttpMessageHandler();
        var jsonBody = "{ \"error\": \"Domain not found\" }";
        httpClientMock.When("https://api.transip.nl/v6/domains/*/dns")
            .Respond(HttpStatusCode.NotFound, "application/json", jsonBody);
        
        var sut = new TransIpService(httpClientMock.ToHttpClient());

        // Act
        var result = async () => await sut.GetDnsEntriesForDomainAsync(domain, CancellationToken);

        // Assert
        await result.Should().ThrowAsync<DnsProviderException>().WithMessage($"*{jsonBody}*");
    }
    
    [Fact]
    public async Task GetDnsEntriesForDomainAsync_ApiReturnedMalformedJson_ShouldThrowException()
    {
        // Arrange
        var domain = Fixture.Create<string>();
        var httpClientMock = new MockHttpMessageHandler();
        httpClientMock.When("https://api.transip.nl/v6/domains/*/dns")
            .Respond("application/json", "{}");
        
        var sut = new TransIpService(httpClientMock.ToHttpClient());

        // Act
        var result = async () => await sut.GetDnsEntriesForDomainAsync(domain, CancellationToken);

        // Assert
        await result.Should().ThrowAsync<Exception>();
    }
    
    [Fact]
    public async Task UpdateDnsEntriesAsync_ForDnsEntry_ShouldMakeHttpRequest()
    {
        // Arrange
        var domain = Fixture.Create<string>();
        
        var dnsEntry = new DnsEntry
        {
            Content = Fixture.Create<string>(),
            ExpiresAt = Fixture.Create<int>(),
            Name = Fixture.Create<string>(),
            Type = Fixture.Create<string>()
        };
        
        var mappedTransIpDnsEntry = new PublicDnsUpdater.Services.Dns.TransIP.Requests.DnsEntry
        {
            Content = dnsEntry.Content,
            Name = dnsEntry.Name,
            Expire = dnsEntry.ExpiresAt,
            Type = dnsEntry.Type
        };
        
        var httpClientMock = new MockHttpMessageHandler();
        var request = httpClientMock.When("https://api.transip.nl/v6/domains/*/dns")
            .WithJsonContent(new UpdateDnsEntryRequestBody(mappedTransIpDnsEntry), new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
            .Respond(HttpStatusCode.NoContent);
        
        var sut = new TransIpService(httpClientMock.ToHttpClient());

        // Act
        var sutAction = async () => await sut.UpdateDnsEntriesAsync(domain, dnsEntry, "2.2.2.2", CancellationToken);

        // Assert
        httpClientMock.AddRequestExpectation(request);
        await sutAction.Should().NotThrowAsync();
    }
    
    [Fact]
    public async Task UpdateDnsEntriesAsync_ForInvalidDnsEntry_ShouldThrowExceptionContainingResponseBody()
    {
        // Arrange
        var domain = Fixture.Create<string>();
        
        var dnsEntry = new DnsEntry
        {
            Content = Fixture.Create<string>(),
            ExpiresAt = Fixture.Create<int>(),
            Name = Fixture.Create<string>(),
            Type = Fixture.Create<string>()
        };
        
        var mappedTransIpDnsEntry = new PublicDnsUpdater.Services.Dns.TransIP.Requests.DnsEntry
        {
            Content = dnsEntry.Content,
            Name = dnsEntry.Name,
            Expire = dnsEntry.ExpiresAt,
            Type = dnsEntry.Type
        };
        
        var httpClientMock = new MockHttpMessageHandler();
        
        var jsonBody = "{ \"error\": \"Conflict!\" }";
        var request = httpClientMock.When("https://api.transip.nl/v6/domains/*/dns")
            .WithJsonContent(new UpdateDnsEntryRequestBody(mappedTransIpDnsEntry), new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
            .Respond(HttpStatusCode.Conflict, "application/json", jsonBody);
        
        var sut = new TransIpService(httpClientMock.ToHttpClient());

        // Act
        var sutAction = async () => await sut.UpdateDnsEntriesAsync(domain, dnsEntry, "2.2.2.2", CancellationToken);

        // Assert
        httpClientMock.AddRequestExpectation(request);
        await sutAction.Should().ThrowAsync<DnsProviderException>().WithMessage($"*{jsonBody}*");
    }
}