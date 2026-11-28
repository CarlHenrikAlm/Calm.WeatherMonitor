using Calm.WeatherMonitor.Core.Results;

namespace Calm.WeatherMonitor.Core.Interfaces
{
    public interface IWeatherApiService
    {
        Task<ApiImportResult> FetchAndSaveAsync();
    }
}
