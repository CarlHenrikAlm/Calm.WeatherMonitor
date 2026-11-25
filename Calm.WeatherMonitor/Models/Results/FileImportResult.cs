namespace Calm.WeatherMonitor.Models.Results;

public sealed class FileImportResult
{
    public int ImportedRows { get; set; }

    public int RejectedRows { get; set; }

    public int DuplicateRows { get; set; }

    public int ProcessedRows =>
        ImportedRows + RejectedRows + DuplicateRows;
}