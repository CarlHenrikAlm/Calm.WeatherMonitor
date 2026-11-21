using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Calm.WeatherMonitor.Data;

public class WeatherDbContextFactory
    : IDesignTimeDbContextFactory<WeatherDbContext>
{
    public WeatherDbContext CreateDbContext(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(
                "appsettings.json",
                optional: false,
                reloadOnChange: false)
            .Build();

        string connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");

        var optionsBuilder =
            new DbContextOptionsBuilder<WeatherDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new WeatherDbContext(optionsBuilder.Options);
    }
}