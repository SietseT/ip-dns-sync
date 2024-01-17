namespace PublicDnsUpdater.Core.Abstractions;

public interface IExternalIpService
{
    Task<string?> GetExternalIpAsync();
}