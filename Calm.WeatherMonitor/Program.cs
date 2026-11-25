using Calm.WeatherMonitor.Data;
using Calm.WeatherMonitor.Services;
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

var menu =
    new ApplicationMenu(
        databaseStatusService,
        fileImportService);

await menu.RunAsync();