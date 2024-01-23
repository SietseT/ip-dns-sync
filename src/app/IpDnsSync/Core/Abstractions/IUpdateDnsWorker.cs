using IpDnsSync.Configuration;
using Microsoft.Extensions.Hosting;

namespace IpDnsSync.Core.Abstractions;

public interface IUpdateDnsWorker : IHostedService
{
    public Provider Provider { get; }
}