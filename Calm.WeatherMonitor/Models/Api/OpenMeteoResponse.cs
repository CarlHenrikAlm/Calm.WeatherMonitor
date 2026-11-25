using System.Text.Json.Serialization;

namespace Calm.WeatherMonitor.Models.Api;

public sealed class OpenMeteoResponse
{
    [JsonPropertyName("current")]
    public OpenMeteoCurrent? Current { get; set; }
}