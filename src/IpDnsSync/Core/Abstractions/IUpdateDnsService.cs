namespace IpDnsSync.Core.Abstractions;

public interface IUpdateDnsService
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}