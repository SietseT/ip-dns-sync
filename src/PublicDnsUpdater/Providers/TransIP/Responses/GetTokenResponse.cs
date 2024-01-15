namespace PublicDnsUpdater.Providers.TransIP.Responses;

public record GetTokenResponse
{
    public string? Token { get; init; }
}