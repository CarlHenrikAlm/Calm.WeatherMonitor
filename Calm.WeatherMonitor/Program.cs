using Calm.WeatherMonitor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ApplicationMenu =
    Calm.WeatherMonitor.App.Menus.Menu;


IConfiguration configuration =
    new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile(
            "appsettings.json",
            optional: false,
            reloadOnChange: false)
        .Build();

var services =
    new ServiceCollection();

services.AddWeatherMonitorServices(
    configuration);

services.AddScoped<ApplicationMenu>();

using ServiceProvider serviceProvider =
    services.BuildServiceProvider(
        new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });

using IServiceScope scope =
    serviceProvider.CreateScope();

ApplicationMenu menu =
    scope.ServiceProvider
        .GetRequiredService<ApplicationMenu>();

await menu.RunAsync();