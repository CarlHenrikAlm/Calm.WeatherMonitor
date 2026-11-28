using Calm.WeatherMonitor.Core.Entities;
using Calm.WeatherMonitor.Core.Exceptions;
using Calm.WeatherMonitor.Core.Interfaces;
using Calm.WeatherMonitor.Core.Results;
using Calm.WeatherMonitor.Services.Data;
using Calm.WeatherMonitor.Services.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net.Http.Json;

namespace Calm.WeatherMonitor.Services.Api;

public sealed class WeatherApiService : IWeatherApiService
{
    private const string StationName =
        "Goteborg_OpenMeteo";

    private const string RequestUri =
        "v1/forecast" +
        "?latitude=57.6832" +
        "&longitude=11.9511" +
        "&current=temperature_2m,relative_humidity_2m" +
        "&timezone=Europe%2FStockholm";

    private const string TimestampFormat =
        "yyyy-MM-dd'T'HH:mm";

    private const double MinimumTemperature = -100.0;
    private const double MaximumTemperature = 60.0;

    private const double MinimumHumidity = 0.0;
    private const double MaximumHumidity = 100.0;

    private readonly HttpClient _httpClient;
    private readonly DbContextOptions<WeatherDbContext> _options;

    public WeatherApiService(
        HttpClient httpClient,
        DbContextOptions<WeatherDbContext> options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);

        _httpClient = httpClient;
        _options = options;
    }

    public async Task<ApiImportResult> FetchAndSaveAsync()
    {
        OpenMeteoResponse? response =
            await _httpClient.GetFromJsonAsync<OpenMeteoResponse>(
                RequestUri);

        OpenMeteoCurrent current =
            response?.Current
            ?? throw new InvalidDataException(
                "The weather API response did not contain current weather data.");

        if (!DateTime.TryParseExact(
                current.Time,
                TimestampFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime timestamp))
        {
            throw new InvalidDataException(
                "The weather API returned an invalid timestamp.");
        }

        if (!IsTemperatureReasonable(current.Temperature))
        {
            throw new InvalidDataException(
                "The weather API returned an unreasonable temperature.");
        }

        if (!IsHumidityReasonable(current.Humidity))
        {
            throw new InvalidDataException(
                "The weather API returned an unreasonable humidity value.");
        }

        await using var dbContext =
            new WeatherDbContext(_options);

        bool exists =
            await dbContext.WeatherDataPoints.AnyAsync(
                dataPoint =>
                    dataPoint.StationName == StationName &&
                    dataPoint.Timestamp == timestamp);

        if (!exists)
        {
            var dataPoint = new WeatherDataPoint
            {
                StationName = StationName,
                Timestamp = timestamp,
                Temperature = current.Temperature,
                Humidity = current.Humidity,
                DataSource = "API"
            };

            try
            {
                await dbContext.WeatherDataPoints.AddAsync(
                    dataPoint);

                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                throw new ServiceOperationException(
                    "The live weather measurement could not be saved.",
                    exception);
            }
        }

        return new ApiImportResult
        {
            StationName = StationName,
            Timestamp = timestamp,
            Temperature = current.Temperature,
            Humidity = current.Humidity,
            WasSaved = !exists
        };
    }

    private static bool IsTemperatureReasonable(
        double temperature)
    {
        return double.IsFinite(temperature)
            && temperature is
                >= MinimumTemperature
                and <= MaximumTemperature;
    }

    private static bool IsHumidityReasonable(
        double humidity)
    {
        return double.IsFinite(humidity)
            && humidity is
                >= MinimumHumidity
                and <= MaximumHumidity;
    }
}