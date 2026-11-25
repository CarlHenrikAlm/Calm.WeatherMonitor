namespace Calm.WeatherMonitor.Classes.Menu.States;

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

    public List<string> LogEntries { get; } = [];

}