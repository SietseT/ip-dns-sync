using System.Text.Json;
using PublicDnsUpdater.Helpers;

namespace PublicDnsUpdater.Configuration;

public static class JsonSerializerConfiguration
{
    public static JsonSerializerOptions SnakeCase = new()
    {
        PropertyNamingPolicy = new SnakeCaseNamingPolicy()
    };
    
    public static JsonSerializerOptions CamelCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}