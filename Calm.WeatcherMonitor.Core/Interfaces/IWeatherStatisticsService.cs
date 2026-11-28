using Calm.WeatherMonitor.Core.Results;

namespace Calm.WeatherMonitor.Core.Interfaces
{
    public interface IWeatherStatisticsService
    {
        Task<IReadOnlyList<string>>
        GetStationNamesAsync();

        Task<StationStatisticsResult?>
            GetStatisticsAsync(
                string stationName);
    }
}
