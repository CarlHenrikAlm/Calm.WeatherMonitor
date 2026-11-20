using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Calm.WeatherMonitor.Data;

public class WeatherDbContextFactory
    : IDesignTimeDbContextFactory<WeatherDbContext>
{
    public WeatherDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<WeatherDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=.\\SQLEXPRESS;Database=CalmWeatherMonitorDb;Trusted_Connection=True;TrustServerCertificate=True;");

        return new WeatherDbContext(optionsBuilder.Options);
    }
}