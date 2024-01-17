// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace PublicDnsUpdater.Services.Dns.TransIP.Requests;

public record GetTokenBody
{
    public required string Login { get; set; }
    public required string Nonce { get; set; }
    public required bool ReadOnly { get; set; }
    public required string ExpirationTime { get; set; }
    public required string Label { get; set; }
    public required bool GlobalKey { get; set; }
}