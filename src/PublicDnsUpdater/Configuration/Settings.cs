namespace PublicDnsUpdater.Configuration;

public record Settings
{
    public bool TestMode { get; init; }
    public string? TestModeIpAddress { get; init; }
}