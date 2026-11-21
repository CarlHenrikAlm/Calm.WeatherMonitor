using Calm.WeatherMonitor.Classes.Menu.Factory;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Calm.WeatherMonitor.Classes.Menu.Panels;

public static class MenuPanelBuilder
{
    private static readonly string[] MenuItems =
    [
        "Import data from file",
        "Fetch live data from API",
        "View weather statistics",
        "Exit"
    ];

    public static Panel Build(MenuState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        int selectedIndex = Math.Clamp(
            state.SelectedIndex,
            0,
            MenuItems.Length - 1);

        var rows = new List<IRenderable>();

        for (int index = 0; index < MenuItems.Length; index++)
        {
            bool isSelected = index == selectedIndex;
            string menuItem = Markup.Escape(MenuItems[index]);

            string row = isSelected
                ? $"[black on deepskyblue1]> {menuItem}[/]"
                : $"[grey]  {menuItem}[/]";

            rows.Add(new Markup(row));
        }

        return PanelFactory.Create(
            "MAIN MENU",
            new Rows(rows),
            Color.DeepSkyBlue1);
    }
}