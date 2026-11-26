using Calm.WeatherMonitor.Services.Statistics;
using Calm.WeatherMonitor.Data;
using Calm.WeatherMonitor.Services;
using Calm.WeatherMonitor.Services.Api;
using Calm.WeatherMonitor.Services.FileImport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ApplicationMenu =
    Calm.WeatherMonitor.Classes.Menu.Menu;

IConfiguration configuration =
    new ConfigurationBuilder()
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

var databaseStatusService =
    new DatabaseStatusService(
        optionsBuilder.Options);

var badDataLogger =
    new BadDataLogger();

var fileImportService =
    new FileImportService(
        optionsBuilder.Options,
        badDataLogger);

using var httpClient = new HttpClient
{
    BaseAddress = new Uri(
        "https://api.open-meteo.com/"),

    Timeout = TimeSpan.FromSeconds(15)
};

var weatherApiService =
    new WeatherApiService(
        httpClient,
        optionsBuilder.Options);

var weatherStatisticsService =
    new WeatherStatisticsService(
        optionsBuilder.Options);

var menu =
    new ApplicationMenu(
        databaseStatusService,
        fileImportService,
        weatherApiService,
        weatherStatisticsService);

await menu.RunAsync();