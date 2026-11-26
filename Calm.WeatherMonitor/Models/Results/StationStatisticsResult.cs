namespace Calm.WeatherMonitor.Models.Results;

public sealed class StationStatisticsResult
{
    public required string StationName { get; init; }

    public required IReadOnlyList<WeatherMeasurementResult>
        Measurements
    { get; init; }

    public double MinimumTemperature { get; init; }

    public double MaximumTemperature { get; init; }

    public double AverageTemperature { get; init; }

    public int MeasurementCount =>
        Measurements.Count;
}