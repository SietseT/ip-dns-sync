namespace PublicDnsUpdater.Services.Dns.TransIP.Responses;

public record GetTokenResponse
{
    public string? Token { get; init; }
}