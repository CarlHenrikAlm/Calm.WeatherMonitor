

namespace Calm.WeatherMonitor.Classes.Menu;

public class MenuState
{
    public MenuMode Mode { get; set; }
        = MenuMode.MainMenu;

    public string CurrentScreen { get; set; }
        = "Home";

    public int SelectedIndex { get; set; }

    public List<string> LogEntries { get; } = [];
}