using Calm.WeatherMonitor.Data;
using Microsoft.EntityFrameworkCore;

namespace Calm.WeatherMonitor.Services;

public sealed class DatabaseStatusService
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