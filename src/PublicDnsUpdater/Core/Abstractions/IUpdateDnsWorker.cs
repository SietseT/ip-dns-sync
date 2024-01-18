using Microsoft.Extensions.Hosting;
using PublicDnsUpdater.Configuration;

namespace PublicDnsUpdater.Core.Abstractions;

public interface IUpdateDnsWorker : IHostedService
{
    public Provider Provider { get; }
}