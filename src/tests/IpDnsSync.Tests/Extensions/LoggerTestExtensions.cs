using Microsoft.Extensions.Logging;
using NSubstitute;

namespace IpDnsSync.Tests.Extensions;

public static class LoggerTestExtensions
{
    public static void AnyLogOfType<T>(this ILogger<T> logger, LogLevel level) where T : class
    {
        logger.Log(level, Arg.Any<EventId>(), Arg.Any<object>(), Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}