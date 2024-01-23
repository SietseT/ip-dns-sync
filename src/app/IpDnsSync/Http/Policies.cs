using Polly;
using Polly.Extensions.Http;

namespace IpDnsSync.Http;

internal static class Policies
{
    internal static IAsyncPolicy<HttpResponseMessage> GetDefaultRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}