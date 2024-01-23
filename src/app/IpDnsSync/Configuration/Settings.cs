namespace IpDnsSync.Configuration;

public record Settings
{
    public bool TestMode { get; init; }
    public string? TestModeIpAddress { get; init; }
    public double UpdateIntervalInMinutes { get; init; } = 5;
}