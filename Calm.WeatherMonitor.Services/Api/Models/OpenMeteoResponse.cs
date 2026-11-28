using System.Text.Json.Serialization;

namespace Calm.WeatherMonitor.Services.Api.Models;

public sealed class OpenMeteoResponse
{
    [JsonPropertyName("current")]
    public OpenMeteoCurrent? Current { get; set; }
}