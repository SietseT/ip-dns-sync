namespace IpDnsSync.Configuration;

public record Provider
{
    private Provider(string value)
    {
        Value = value;
    }

    internal static Provider TransIp = new("TransIP");

    private string Value { get; }
    
    public static implicit operator string(Provider provider) { return provider.ToString(); }
    public override string ToString()
    {
        return Value;
    }
}