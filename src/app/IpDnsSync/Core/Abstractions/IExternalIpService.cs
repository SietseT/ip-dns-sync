namespace IpDnsSync.Core.Abstractions;

public interface IExternalIpService
{
    Task<string?> GetExternalIpAsync();
}