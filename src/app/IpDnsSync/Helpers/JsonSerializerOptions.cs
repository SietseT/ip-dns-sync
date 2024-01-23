using System.Text.Json;

namespace IpDnsSync.Helpers;

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