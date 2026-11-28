namespace Calm.WeatherMonitor.Core.Results;

public sealed class WeatherMeasurementResult
{
    public DateTime Timestamp { get; init; }

    public double Temperature { get; init; }

    public double Humidity { get; init; }

    public required string DataSource { get; init; }
}