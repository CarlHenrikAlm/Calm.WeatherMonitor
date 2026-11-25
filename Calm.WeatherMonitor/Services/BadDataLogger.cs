using System.Text;

namespace Calm.WeatherMonitor.Services.FileImport;

public sealed class BadDataLogger
{
    private const string LogFileName = "bad_data.log";

    private readonly string _logFilePath;

    public BadDataLogger()
    {
        _logFilePath = Path.Combine(
            AppContext.BaseDirectory,
            LogFileName);
    }

    public string LogFilePath => _logFilePath;

    public async Task ResetAsync()
    {
        await File.WriteAllTextAsync(
            _logFilePath,
            string.Empty,
            Encoding.UTF8);
    }

    public async Task WriteRejectedLinesAsync(
        IEnumerable<string> rejectedLines)
    {
        ArgumentNullException.ThrowIfNull(rejectedLines);

        await File.WriteAllLinesAsync(
            _logFilePath,
            rejectedLines,
            Encoding.UTF8);
    }
}