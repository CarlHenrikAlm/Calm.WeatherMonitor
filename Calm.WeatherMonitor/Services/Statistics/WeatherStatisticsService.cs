using Calm.WeatherMonitor.Data;
using Calm.WeatherMonitor.Models.Results;
using Microsoft.EntityFrameworkCore;

namespace Calm.WeatherMonitor.Services.Statistics;

public sealed class WeatherStatisticsService
{
    private readonly DbContextOptions<WeatherDbContext> _options;

    public WeatherStatisticsService(
        DbContextOptions<WeatherDbContext> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    public async Task<IReadOnlyList<string>>
        GetStationNamesAsync()
    {
        await using var dbContext =
            new WeatherDbContext(_options);

        List<string> stationNames =
            await dbContext.WeatherDataPoints
                .AsNoTracking()
                .Select(dataPoint =>
                    dataPoint.StationName)
                .Distinct()
                .OrderBy(stationName =>
                    stationName)
                .ToListAsync();

        return stationNames;
    }

    public async Task<StationStatisticsResult?>
        GetStatisticsAsync(string stationName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            stationName);

        await using var dbContext =
            new WeatherDbContext(_options);

        List<WeatherMeasurementResult> measurements =
            await dbContext.WeatherDataPoints
                .AsNoTracking()
                .Where(dataPoint =>
                    dataPoint.StationName == stationName)
                .OrderBy(dataPoint =>
                    dataPoint.Timestamp)
                .Select(dataPoint =>
                    new WeatherMeasurementResult
                    {
                        Timestamp =
                            dataPoint.Timestamp,

                        Temperature =
                            dataPoint.Temperature,

                        Humidity =
                            dataPoint.Humidity,

                        DataSource =
                            dataPoint.DataSource
                    })
                .ToListAsync();

        if (measurements.Count == 0)
        {
            return null;
        }

        return new StationStatisticsResult
        {
            StationName = stationName,
            Measurements = measurements,

            MinimumTemperature =
                measurements.Min(
                    measurement =>
                        measurement.Temperature),

            MaximumTemperature =
                measurements.Max(
                    measurement =>
                        measurement.Temperature),

            AverageTemperature =
                measurements.Average(
                    measurement =>
                        measurement.Temperature)
        };
    }
}