using Calm.WeatherMonitor.Core.Results;

namespace Calm.WeatherMonitor.App.Menus.States;


public class MenuState
{
    public MenuMode Mode { get; set; }
        = MenuMode.MainMenu;

    public string CurrentScreen { get; set; }
        = "Home";

    public string? InformationMessage { get; set; }

    public int SelectedIndex { get; set; }

    public DatabaseConnectionStatus DatabaseStatus { get; set; }
         = DatabaseConnectionStatus.NotChecked;

    public StationStatisticsResult? StatisticsResult { get; set; }

    public int StatisticsPageIndex { get; set; }

    public IReadOnlyList<string> StationNames { get; set; }
    = [];

    public int SelectedStationIndex { get; set; }

    public bool IsSelectingStation { get; set; }

    public List<string> LogEntries { get; } = [];

}