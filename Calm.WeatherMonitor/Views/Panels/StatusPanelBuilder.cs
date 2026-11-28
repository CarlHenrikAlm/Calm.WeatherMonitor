using Calm.WeatherMonitor.App.Menus.States;
using Calm.WeatherMonitor.App.Views.Factory;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Calm.WeatherMonitor.App.Views.Panels;

public static class StatusPanelBuilder
{
    public static Panel Build(MenuState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        string currentScreen =
            Markup.Escape(state.CurrentScreen);

        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap())
            .AddColumn();

        grid.AddRow(
            new Markup("[grey]Application[/]"),
            new Markup("[green]Running[/]"));

        grid.AddRow(
            new Markup("[grey]Database[/]"),
            CreateDatabaseStatus(state.DatabaseStatus));

        grid.AddRow(
            new Markup("[grey]Current screen[/]"),
            new Markup(
                $"[deepskyblue1]{currentScreen}[/]"));

        return PanelFactory.Create(
            "SYSTEM STATUS",
            grid,
            Color.Green);
    }

    private static IRenderable CreateDatabaseStatus(
        DatabaseConnectionStatus status)
    {
        return status switch
        {
            DatabaseConnectionStatus.Connected =>
                new Markup("[green]Connected[/]"),

            DatabaseConnectionStatus.Unavailable =>
                new Markup("[red]Unavailable[/]"),

            _ =>
                new Markup("[yellow]Not checked[/]")
        };
    }
}