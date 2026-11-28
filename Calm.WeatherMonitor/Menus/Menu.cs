using Calm.WeatherMonitor.App.Views.Layouts;
using Calm.WeatherMonitor.App.Menus.States;
using Calm.WeatherMonitor.Core.Exceptions;
using Calm.WeatherMonitor.Core.Results;
using Calm.WeatherMonitor.Core.Interfaces;
using Spectre.Console;


namespace Calm.WeatherMonitor.App.Menus;

public sealed class Menu
{
    private const int MenuItemCount = 4;
    private const int MinimumStatisticsPageSize = 2;
    private const int MaximumStatisticsPageSize = 20;
    private const int ReservedStatisticsRows = 23;

    private readonly MenuState _state;
    private readonly IDatabaseStatusService _databaseStatusService;
    private readonly IFileImportService _fileImportService;
    private readonly IWeatherApiService _weatherApiService;
    private readonly IWeatherStatisticsService _weatherStatisticsService;

    private bool _isRunning = true;

    public Menu(
        IDatabaseStatusService databaseStatusService,
        IFileImportService fileImportService,
        IWeatherApiService weatherApiService,
        IWeatherStatisticsService weatherStatisticsService)
    {
        ArgumentNullException.ThrowIfNull(
            databaseStatusService);

        ArgumentNullException.ThrowIfNull(
            fileImportService);
        
        ArgumentNullException.ThrowIfNull(
            weatherApiService);
        
        ArgumentNullException.ThrowIfNull(
            weatherStatisticsService);

        _databaseStatusService = databaseStatusService;
        _fileImportService = fileImportService;
        _weatherApiService = weatherApiService;
        _weatherStatisticsService = weatherStatisticsService;

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
        try
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
        catch (Exception)
        {
            _state.DatabaseStatus =
                DatabaseConnectionStatus.Unavailable;
            _state.LogEntries.Add(
                "Database connection check failed.");
        }
    }

    private void Render()
    {
        UpdateStatisticsPageSize();

        Layout dashboard =
            DashboardLayout.Build(_state);

        AnsiConsole.Clear();
        AnsiConsole.Write(dashboard);
    }

    private void UpdateStatisticsPageSize()
    {
        if (_state.Mode != MenuMode.Statistics
            || _state.StatisticsResult is null)
        {
            return;
        }

        int availableRows =
            Console.WindowHeight
            - ReservedStatisticsRows;

        _state.StatisticsPageSize =
            Math.Clamp(
                availableRows,
                MinimumStatisticsPageSize,
                MaximumStatisticsPageSize);

        ClampStatisticsPageIndex();
    }

    private void ClampStatisticsPageIndex()
    {
        StationStatisticsResult? statistics =
            _state.StatisticsResult;

        if (statistics is null)
        {
            _state.StatisticsPageIndex = 0;

            return;
        }

        int pageCount =
            CalculateStatisticsPageCount(
                statistics.MeasurementCount);

        _state.StatisticsPageIndex =
            Math.Clamp(
                _state.StatisticsPageIndex,
                0,
                pageCount - 1);
    }

    private int CalculateStatisticsPageCount(
    int measurementCount)
    {
        int pageSize =
            Math.Max(
                1,
                _state.StatisticsPageSize);

        return Math.Max(
            1,
            (int)Math.Ceiling(
                measurementCount
                / (double)pageSize));
    }

    private async Task HandleKeyAsync(ConsoleKey key)
    {
        if (_state.IsSelectingStation)
        {
            await HandleStationSelectionKeyAsync(key);

            return;
        }

        if (_state.StatisticsResult is not null)
        {
            await HandleStatisticsKeyAsync(key);

            return;
        }

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

    private async Task HandleStationSelectionKeyAsync(
    ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.UpArrow:
                MoveStationSelectionUp();
                break;

            case ConsoleKey.DownArrow:
                MoveStationSelectionDown();
                break;

            case ConsoleKey.Enter:
                await LoadSelectedStationStatisticsAsync();
                break;

            case ConsoleKey.Escape:
                ReturnToMainMenu();
                break;
        }
    }

    private async Task LoadSelectedStationStatisticsAsync()
    {
        if (_state.StationNames.Count == 0)
        {
            SetStatisticsError(
                "No weather station is available.");

            return;
        }

        string selectedStation =
            _state.StationNames[
                _state.SelectedStationIndex];

        _state.IsSelectingStation = false;
        _state.InformationMessage =
            $"Loading statistics for " +
            $"{selectedStation}...";

        Render();

        try
        {
            StationStatisticsResult? statistics =
                await _weatherStatisticsService
                    .GetStatisticsAsync(
                        selectedStation);

            if (statistics is null)
            {
                SetStatisticsError(
                    "No measurements were found for " +
                    "the selected station.");

                return;
            }

            _state.StatisticsResult = statistics;
            _state.StatisticsPageIndex = 0;
            _state.InformationMessage = null;

            _state.LogEntries.Add(
                $"Statistics loaded for " +
                $"{selectedStation}: " +
                $"{statistics.MeasurementCount} " +
                "measurements.");
        }
        catch (Exception)
        {
            SetStatisticsError(
                "The weather statistics could not be loaded.");
        }
    }

    private async Task HandleStatisticsKeyAsync(
    ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.LeftArrow:
                MoveStatisticsPage(-1);
                break;

            case ConsoleKey.RightArrow:
                MoveStatisticsPage(1);
                break;

            case ConsoleKey.Enter:
                _state.StatisticsResult = null;
                _state.StatisticsPageIndex = 0;
                _state.IsSelectingStation = true;
                break;

            case ConsoleKey.Escape:
                ReturnToMainMenu();
                break;
        }

        await Task.CompletedTask;
    }

    private void MoveStatisticsPage(int direction)
    {
        StationStatisticsResult? statistics =
            _state.StatisticsResult;

        if (statistics is null)
        {
            return;
        }

        int pageCount =
            CalculateStatisticsPageCount(
                statistics.MeasurementCount);

        _state.StatisticsPageIndex =
            Math.Clamp(
                _state.StatisticsPageIndex + direction,
                0,
                pageCount - 1);
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

    private void MoveStationSelectionUp()
    {
        if (_state.StationNames.Count == 0)
        {
            return;
        }

        _state.SelectedStationIndex =
            (_state.SelectedStationIndex - 1
                + _state.StationNames.Count)
            % _state.StationNames.Count;
    }

    private void MoveStationSelectionDown()
    {
        if (_state.StationNames.Count == 0)
        {
            return;
        }

        _state.SelectedStationIndex =
            (_state.SelectedStationIndex + 1)
            % _state.StationNames.Count;
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
                await ShowStatisticsAsync();
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
        catch (ServiceOperationException exception)
        {
            SetImportError(
                exception.Message);
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
        catch (ServiceOperationException exception)
        {
            SetApiError(
                exception.Message);
        }
        catch (Exception)
        {
            SetApiError(
                "An unexpected error occurred while fetching live weather data.");
        }
    }

    private async Task ShowStatisticsAsync()
    {
        _state.Mode = MenuMode.Statistics;
        _state.CurrentScreen = "Statistics";
        _state.InformationMessage =
            "Loading weather stations...";
        _state.StatisticsResult = null;
        _state.StatisticsPageIndex = 0;
        _state.StationNames = [];
        _state.SelectedStationIndex = 0;
        _state.IsSelectingStation = false;

        Render();

        try
        {
            IReadOnlyList<string> stationNames =
                await _weatherStatisticsService
                    .GetStationNamesAsync();

            if (stationNames.Count == 0)
            {
                SetStatisticsError(
                    "No weather stations are stored " +
                    "in the database.");

                return;
            }

            _state.StationNames = stationNames;
            _state.SelectedStationIndex = 0;
            _state.IsSelectingStation = true;
            _state.InformationMessage = null;

            _state.LogEntries.Add(
                $"{stationNames.Count} weather stations loaded.");
        }
        catch (Exception)
        {
            SetStatisticsError(
                "The weather stations could not be loaded.");
        }
    }

    private void SetStatisticsError(string message)
    {
        _state.StatisticsResult = null;
        _state.StatisticsPageIndex = 0;
        _state.StationNames = [];
        _state.SelectedStationIndex = 0;
        _state.IsSelectingStation = false;

        _state.InformationMessage =
            $"Statistics unavailable." +
            Environment.NewLine +
            Environment.NewLine +
            message;

        _state.LogEntries.Add(
            $"Statistics failed: {message}");
    }

    private void ReturnToMainMenu()
    {
        _state.Mode = MenuMode.MainMenu;
        _state.CurrentScreen = "Home";
        _state.InformationMessage = null;
        _state.StatisticsResult = null;
        _state.StatisticsPageIndex = 0;
        _state.StationNames = [];
        _state.SelectedStationIndex = 0;
        _state.IsSelectingStation = false;
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