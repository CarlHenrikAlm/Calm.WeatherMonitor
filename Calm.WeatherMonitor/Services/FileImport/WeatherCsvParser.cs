using Calm.WeatherMonitor.Models;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Calm.WeatherMonitor.Services.FileImport;

public static class WeatherCsvParser
{
    private const int ExpectedFieldCount = 4;

    private const double MinimumTemperature = -100.0;
    private const double MaximumTemperature = 60.0;

    private const double MinimumHumidity = 0.0;
    private const double MaximumHumidity = 100.0;

    private const string TimestampFormat =
        "yyyy-MM-dd HH:mm:ss";

    public static bool TryParse(
        string rawLine,
        [NotNullWhen(true)]
    out WeatherDataPoint? weatherDataPoint)
    {
        weatherDataPoint = null;

        if (string.IsNullOrWhiteSpace(rawLine))
        {
            return false;
        }

        string[] fields = rawLine.Split('\t');

        if (fields.Length != ExpectedFieldCount)
        {
            return false;
        }

        string stationName = fields[0].Trim();
        string timestampText = fields[1].Trim();
        string temperatureText =
            NormalizeDecimal(fields[2]);
        string humidityText =
            NormalizeDecimal(fields[3]);

        if (string.IsNullOrWhiteSpace(stationName))
        {
            return false;
        }

        if (!DateTime.TryParseExact(
                timestampText,
                TimestampFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime timestamp))
        {
            return false;
        }

        if (!TryParseNumber(
                temperatureText,
                out double temperature))
        {
            return false;
        }

        if (!TryParseNumber(
                humidityText,
                out double humidity))
        {
            return false;
        }

        if (!IsTemperatureReasonable(temperature))
        {
            return false;
        }

        if (!IsHumidityReasonable(humidity))
        {
            return false;
        }

        weatherDataPoint = new WeatherDataPoint
        {
            StationName = stationName,
            Timestamp = timestamp,
            Temperature = temperature,
            Humidity = humidity,
            DataSource = "CSV-File"
        };

        return true;
    }

    private static string NormalizeDecimal(string value)
    {
        return value
            .Trim()
            .Replace(',', '.');
    }

    private static bool TryParseNumber(
        string value,
        out double number)
    {
        return double.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out number)
            && double.IsFinite(number);
    }

    private static bool IsTemperatureReasonable(
        double temperature)
    {
        return temperature is
            >= MinimumTemperature
            and <= MaximumTemperature;
    }

    private static bool IsHumidityReasonable(
        double humidity)
    {
        return humidity is
            >= MinimumHumidity
            and <= MaximumHumidity;
    }
}