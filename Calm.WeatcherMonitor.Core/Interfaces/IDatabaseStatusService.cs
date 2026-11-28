namespace Calm.WeatherMonitor.Core.Interfaces
{
    public interface IDatabaseStatusService
    {
        Task<bool> CanConnectAsync();
    }
}
