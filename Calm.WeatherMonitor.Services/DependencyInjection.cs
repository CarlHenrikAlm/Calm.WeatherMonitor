using Calm.WeatherMonitor.Core.Interfaces;
using Calm.WeatherMonitor.Services.Api;
using Calm.WeatherMonitor.Services.Data;
using Calm.WeatherMonitor.Services.Database;
using Calm.WeatherMonitor.Services.FileImport;
using Calm.WeatherMonitor.Services.Logging;
using Calm.WeatherMonitor.Services.Statistics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Calm.WeatherMonitor.Services;

public static class DependencyInjection
{
    public static IServiceCollection
        AddWeatherMonitorServices(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string connectionString =
            configuration.GetConnectionString(
                "DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string " +
                "'DefaultConnection' was not found.");

        services.AddDbContext<WeatherDbContext>(
            options =>
            {
                options.UseSqlServer(
                    connectionString);
            });

        services.AddTransient<
            IDatabaseStatusService,
            DatabaseStatusService>();

        services.AddTransient<
            IFileImportService,
            FileImportService>();

        services.AddTransient<
            IWeatherStatisticsService,
            WeatherStatisticsService>();

        services.AddSingleton<BadDataLogger>();

        services.AddHttpClient<
            IWeatherApiService,
            WeatherApiService>(
            httpClient =>
            {
                httpClient.BaseAddress =
                    new Uri(
                        "https://api.open-meteo.com/");

                httpClient.Timeout =
                    TimeSpan.FromSeconds(15);
            });

        return services;
    }
}