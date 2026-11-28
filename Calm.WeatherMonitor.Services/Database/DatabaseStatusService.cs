using Calm.WeatherMonitor.Core.Interfaces;
using Calm.WeatherMonitor.Services.Data;
using Microsoft.EntityFrameworkCore;


namespace Calm.WeatherMonitor.Services.Database;

public sealed class DatabaseStatusService : IDatabaseStatusService
{
    private readonly DbContextOptions<WeatherDbContext> _options;

    public DatabaseStatusService(
        DbContextOptions<WeatherDbContext> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    public async Task<bool> CanConnectAsync()
    {
        await using var dbContext =
            new WeatherDbContext(_options);

        return await dbContext.Database.CanConnectAsync();
    }
}