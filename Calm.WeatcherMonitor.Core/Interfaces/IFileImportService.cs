using Calm.WeatherMonitor.Core.Results;

namespace Calm.WeatherMonitor.Core.Interfaces
{
    public interface IFileImportService
    {
        Task<FileImportResult> ImportAsync();
    }
}
