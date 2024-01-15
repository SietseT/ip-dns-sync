namespace PublicDnsUpdater.Core;

public class HttpResponse<T> where T : class
{
    private readonly bool _success;
    private readonly Exception? _exception;
    private readonly T? _data;

    public HttpResponse(T data)
    {
        _success = true;
        _data = data;
    }
    
    public HttpResponse(Exception exception)
    {
        _success = false;
        _exception = exception;
    }
    
    public bool WasSuccessful => _success;
    public T Data => _data ?? throw new InvalidOperationException("Response was not successful -- no data available.");
    public Exception Error => _exception ?? throw new InvalidOperationException("Response was successful -- no exception available.");
}