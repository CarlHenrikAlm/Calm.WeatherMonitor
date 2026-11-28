using Microsoft.EntityFrameworkCore;
using Calm.WeatherMonitor.Core.Entities;
using Calm.WeatherMonitor.Core.Results;
using Calm.WeatherMonitor.Core.Interfaces;
using Calm.WeatherMonitor.Services.Data;
using Calm.WeatherMonitor.Services.Logging;

namespace Calm.WeatherMonitor.Services.FileImport;

public sealed class FileImportService : IFileImportService
{
    private const string DataDirectoryName = "DataFiles";
    private const string DataFileName = "weather_data.csv";

    private readonly DbContextOptions<WeatherDbContext> _options;
    private readonly BadDataLogger _badDataLogger;

    public FileImportService(
        DbContextOptions<WeatherDbContext> options,
        BadDataLogger badDataLogger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(badDataLogger);

        _options = options;
        _badDataLogger = badDataLogger;
    }

    public async Task<FileImportResult> ImportAsync()
    {
        string filePath = GetDataFilePath();

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "The weather data file could not be found.",
                filePath);
        }

        await _badDataLogger.ResetAsync();

        var result = new FileImportResult();
        var rejectedLines = new List<string>();
        var validDataPoints = new List<WeatherDataPoint>();

        await using var dbContext =
            new WeatherDbContext(_options);

        HashSet<WeatherDataKey> knownDataKeys =
            await LoadExistingDataKeysAsync(dbContext);

        bool isHeaderRow = true;

        await foreach (string rawLine in
            File.ReadLinesAsync(filePath))
        {
            if (isHeaderRow)
            {
                isHeaderRow = false;
                continue;
            }

            if (!WeatherCsvParser.TryParse(
                    rawLine,
                    out WeatherDataPoint? dataPoint))
            {
                rejectedLines.Add(rawLine);
                result.RejectedRows++;

                continue;
            }

            var dataKey = new WeatherDataKey(
                dataPoint.StationName,
                dataPoint.Timestamp);

            if (!knownDataKeys.Add(dataKey))
            {
                result.DuplicateRows++;

                continue;
            }

            validDataPoints.Add(dataPoint);
        }

        if (validDataPoints.Count > 0)
        {
            await dbContext.WeatherDataPoints.AddRangeAsync(
                validDataPoints);

            result.ImportedRows =
                await dbContext.SaveChangesAsync();
        }

        await _badDataLogger.WriteRejectedLinesAsync(
            rejectedLines);

        return result;
    }

    private static string GetDataFilePath()
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            DataDirectoryName,
            DataFileName);
    }

    private static async Task<HashSet<WeatherDataKey>>
        LoadExistingDataKeysAsync(
            WeatherDbContext dbContext)
    {
        List<WeatherDataKey> existingKeys =
            await dbContext.WeatherDataPoints
                .AsNoTracking()
                .Select(dataPoint =>
                    new WeatherDataKey(
                        dataPoint.StationName,
                        dataPoint.Timestamp))
                .ToListAsync();

        return existingKeys.ToHashSet();
    }

    private readonly record struct WeatherDataKey(
        string StationName,
        DateTime Timestamp);
}