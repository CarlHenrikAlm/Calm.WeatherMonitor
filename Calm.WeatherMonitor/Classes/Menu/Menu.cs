using Calm.WeatherMonitor.Classes.Menu.Layouts;
using Calm.WeatherMonitor.Classes.Menu.States;
using Calm.WeatherMonitor.Models.Results;
using Calm.WeatherMonitor.Services;
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

    private bool _isRunning = true;

    public Menu(
        DatabaseStatusService databaseStatusService,
        FileImportService fileImportService)
    {
        ArgumentNullException.ThrowIfNull(
            databaseStatusService);

        ArgumentNullException.ThrowIfNull(
            fileImportService);

        _databaseStatusService = databaseStatusService;
        _fileImportService = fileImportService;

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
                SetMode(
                    MenuMode.ApiImport,
                    "API Import",
                    "Live API import selected.");
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