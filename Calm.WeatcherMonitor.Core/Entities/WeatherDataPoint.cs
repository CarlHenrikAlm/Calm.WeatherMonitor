using System.ComponentModel.DataAnnotations;



namespace Calm.WeatherMonitor.Core.Entities;

    public class WeatherDataPoint
    {

    public int Id { get; set; }

    [Required]
    [MaxLength(100)]

    public string StationName { get; set; } =
        string.Empty;

    public DateTime Timestamp { get; set; }
    public double Temperature { get; set; }

    public double Humidity { get; set; }

    [Required]
    [MaxLength(20)]

    public string DataSource { get; set; } =
        string.Empty;
    }

