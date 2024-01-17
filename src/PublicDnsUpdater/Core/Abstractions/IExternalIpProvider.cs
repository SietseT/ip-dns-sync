namespace PublicDnsUpdater.Core.Abstractions;

public interface IExternalIpProvider
{
    Task<string?> GetExternalIpAsync();
}