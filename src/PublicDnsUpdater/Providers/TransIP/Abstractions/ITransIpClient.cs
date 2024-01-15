using PublicDnsUpdater.Core;
using PublicDnsUpdater.Providers.TransIP.Requests;
using PublicDnsUpdater.Providers.TransIP.Responses;

namespace PublicDnsUpdater.Providers.TransIP.Abstractions;

public interface ITransIpClient
{
    Task<HttpResponse<GetTokenResponse>> GetTokenAsync(GetTokenBody body, string jwtSignature, CancellationToken cancellationToken = default);

    Task<HttpResponse<GetDnsEntriesResponse>> GetDnsEntriesAsync(string domain, CancellationToken cancellationToken = default);
    
    Task<HttpResponse<UpdateDnsEntryResponse>> UpdateDnsEntryAsync(string domain, UpdateDnsEntryRequest request, CancellationToken cancellationToken = default);
}