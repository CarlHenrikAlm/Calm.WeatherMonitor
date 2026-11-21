using Calm.WeatherMonitor.Classes.Menu.Factory;
using Spectre.Console;

namespace Calm.WeatherMonitor.Classes.Menu.Panels;

public static class StatusPanelBuilder
{
    public static Panel Build(MenuState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        string currentScreen = Markup.Escape(state.CurrentScreen);

        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap())
            .AddColumn();

        grid.AddRow(
            new Markup("[grey]Application[/]"),
            new Markup("[green]Running[/]"));

        grid.AddRow(
            new Markup("[grey]Database[/]"),
            new Markup("[yellow]Not checked[/]"));

        grid.AddRow(
            new Markup("[grey]Current screen[/]"),
            new Markup($"[deepskyblue1]{currentScreen}[/]"));

        return PanelFactory.Create(
            "SYSTEM STATUS",
            grid,
            Color.Green);
    }
}