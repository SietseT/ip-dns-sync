namespace PublicDnsUpdater.Configuration;

public record ProviderConfiguration<T> where T : new()
{
    public IEnumerable<string> Domains { get; init; } = Array.Empty<string>();
    public T Provider { get; init; } = new();
}