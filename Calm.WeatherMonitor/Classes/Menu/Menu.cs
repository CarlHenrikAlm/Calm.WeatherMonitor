using Calm.WeatherMonitor.Classes.Menu.Layouts;
using Calm.WeatherMonitor.Classes.Menu.States;
using Calm.WeatherMonitor.Models.Results;
using Calm.WeatherMonitor.Services;
using Calm.WeatherMonitor.Services.Api;
using Calm.WeatherMonitor.Services.FileImport;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace Calm.WeatherMonitor.Classes.Menu;

public sealed class Menu
{
    private const int MenuItemCount = 4;

    private readonly MenuState _state;
    private readonly DatabaseStatusService _databaseStatusService;
    private readonly FileImportService _fileImportService;
    private readonly WeatherApiService _weatherApiService;

    private bool _isRunning = true;

    public Menu(
        DatabaseStatusService databaseStatusService,
        FileImportService fileImportService,
        WeatherApiService weatherApiService)
    {
        ArgumentNullException.ThrowIfNull(
            databaseStatusService);

        ArgumentNullException.ThrowIfNull(
            fileImportService);
        
        ArgumentNullException.ThrowIfNull(
            weatherApiService);

        _databaseStatusService = databaseStatusService;
        _fileImportService = fileImportService;
        _weatherApiService = weatherApiService; 

        _state = new MenuState();

        _state.LogEntries.Add(
            "Calm WeatherMonitor started.");
    }

    public async Task RunAsync()
    {
        await CheckDatabaseConnectionAsync();

        while (_isRunning)
        {
            Render();

            ConsoleKeyInfo keyInfo =
                Console.ReadKey(intercept: true);

            await HandleKeyAsync(keyInfo.Key);
        }

        AnsiConsole.Clear();
    }

    private async Task CheckDatabaseConnectionAsync()
    {
        bool canConnect =
            await _databaseStatusService.CanConnectAsync();

        _state.DatabaseStatus = canConnect
            ? DatabaseConnectionStatus.Connected
            : DatabaseConnectionStatus.Unavailable;

        string logEntry = canConnect
            ? "Database connection established."
            : "Database connection unavailable.";

        _state.LogEntries.Add(logEntry);
    }

    private void Render()
    {
        Layout dashboard =
            DashboardLayout.Build(_state);

        AnsiConsole.Clear();
        AnsiConsole.Write(dashboard);
    }

    private async Task HandleKeyAsync(ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.UpArrow:
                MoveSelectionUp();
                break;

            case ConsoleKey.DownArrow:
                MoveSelectionDown();
                break;

            case ConsoleKey.Enter:
                await SelectCurrentItemAsync();
                break;

            case ConsoleKey.Escape:
                Exit();
                break;
        }
    }

    private void MoveSelectionUp()
    {
        _state.SelectedIndex =
            (_state.SelectedIndex - 1 + MenuItemCount)
            % MenuItemCount;
    }

    private void MoveSelectionDown()
    {
        _state.SelectedIndex =
            (_state.SelectedIndex + 1)
            % MenuItemCount;
    }

    private async Task SelectCurrentItemAsync()
    {
        switch (_state.SelectedIndex)
        {
            case 0:
                await ImportFileAsync();
                break;

            case 1:
                await ImportApiDataAsync();
                break;

            case 2:
                SetMode(
                    MenuMode.Statistics,
                    "Statistics",
                    "Weather statistics selected.");
                break;

            case 3:
                Exit();
                break;
        }
    }

    private async Task ImportFileAsync()
    {
        _state.Mode = MenuMode.ImportFile;
        _state.CurrentScreen = "File Import";
        _state.InformationMessage =
            "Importing and validating weather data...";

        Render();

        try
        {
            FileImportResult result =
                await _fileImportService.ImportAsync();

            _state.InformationMessage =
                $"Import completed.{Environment.NewLine}" +
                Environment.NewLine +
                $"Imported rows:  {result.ImportedRows}{Environment.NewLine}" +
                $"Rejected rows:  {result.RejectedRows}{Environment.NewLine}" +
                $"Duplicate rows: {result.DuplicateRows}{Environment.NewLine}" +
                $"Processed rows: {result.ProcessedRows}";

            _state.LogEntries.Add(
                $"File import completed: " +
                $"{result.ImportedRows} imported, " +
                $"{result.RejectedRows} rejected, " +
                $"{result.DuplicateRows} duplicates.");
        }
        catch (FileNotFoundException)
        {
            SetImportError(
                "The weather data file could not be found.");
        }
        catch (UnauthorizedAccessException)
        {
            SetImportError(
                "Access to the weather data file or log file was denied.");
        }
        catch (IOException)
        {
            SetImportError(
                "A file error occurred during the import.");
        }
        catch (DbUpdateException)
        {
            SetImportError(
                "The validated weather data could not be saved.");
        }
        catch (Exception)
        {
            SetImportError(
                "An unexpected error occurred during the import.");
        }
    }

    private async Task ImportApiDataAsync()
    {
        _state.Mode = MenuMode.ApiImport;
        _state.CurrentScreen = "API Import";
        _state.InformationMessage =
            "Fetching current weather data from Open-Meteo...";

        Render();

        try
        {
            ApiImportResult result =
                await _weatherApiService.FetchAndSaveAsync();

            string saveStatus = result.WasSaved
                ? "The measurement was saved to the database."
                : "The measurement already exists and was not saved again.";

            _state.InformationMessage =
                $"Live weather data received.{Environment.NewLine}" +
                Environment.NewLine +
                $"Station:     {result.StationName}{Environment.NewLine}" +
                $"Timestamp:   {result.Timestamp:yyyy-MM-dd HH:mm}{Environment.NewLine}" +
                $"Temperature: {result.Temperature:F1} °C{Environment.NewLine}" +
                $"Humidity:    {result.Humidity:F1} %{Environment.NewLine}" +
                Environment.NewLine +
                saveStatus;

            string logEntry = result.WasSaved
                ? $"API measurement saved for {result.StationName}."
                : $"API measurement skipped as duplicate for {result.StationName}.";

            _state.LogEntries.Add(logEntry);
        }
        catch (TaskCanceledException exception)
            when (exception.InnerException is TimeoutException)
        {
            SetApiError(
                "The weather API request timed out.");
        }
        catch (HttpRequestException)
        {
            SetApiError(
                "The weather API could not be reached.");
        }
        catch (InvalidDataException exception)
        {
            SetApiError(exception.Message);
        }
        catch (DbUpdateException)
        {
            SetApiError(
                "The live weather measurement could not be saved.");
        }
        catch (Exception)
        {
            SetApiError(
                "An unexpected error occurred while fetching live weather data.");
        }
    }

    private void SetApiError(string message)
    {
        _state.InformationMessage =
            $"API import failed.{Environment.NewLine}" +
            Environment.NewLine +
            message;

        _state.LogEntries.Add(
            $"API import failed: {message}");
    }


    private void SetImportError(string message)
    {
        _state.InformationMessage =
            $"Import failed.{Environment.NewLine}" +
            Environment.NewLine +
            message;

        _state.LogEntries.Add(
            $"File import failed: {message}");
    }

    private void SetMode(
        MenuMode mode,
        string currentScreen,
        string logEntry)
    {
        _state.Mode = mode;
        _state.CurrentScreen = currentScreen;
        _state.InformationMessage = null;
        _state.LogEntries.Add(logEntry);
    }

    private void Exit()
    {
        _state.Mode = MenuMode.Exit;
        _state.CurrentScreen = "Exit";
        _state.InformationMessage = null;

        _state.LogEntries.Add(
            "Calm WeatherMonitor closed.");

        _isRunning = false;
    }
}