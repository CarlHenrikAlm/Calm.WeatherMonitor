namespace Calm.WeatherMonitor.Core.Exceptions;

public sealed class ServiceOperationException
    : Exception
{
    public ServiceOperationException(
        string message)
        : base(message)
    {
    }

    public ServiceOperationException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}
