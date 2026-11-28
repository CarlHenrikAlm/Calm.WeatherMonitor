namespace Calm.WeatherMonitor.Core.Results;

public sealed class ApiImportResult
{
    public required string StationName { get; init; }

    public DateTime Timestamp { get; init; }

    public double Temperature { get; init; }

    public double Humidity { get; init; }

    public bool WasSaved { get; init; }
}