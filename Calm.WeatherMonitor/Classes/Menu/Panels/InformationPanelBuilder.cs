using Calm.WeatherMonitor.Classes.Menu.Factory;
using Calm.WeatherMonitor.Classes.Menu.States;
using Spectre.Console;

namespace Calm.WeatherMonitor.Classes.Menu.Panels;

public static class InformationPanelBuilder
{
    public static Panel Build(MenuState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        (string heading, string defaultDescription) =
            GetInformation(state.Mode);

        string description =
            string.IsNullOrWhiteSpace(state.InformationMessage)
                ? defaultDescription
                : state.InformationMessage;

        var content = new Rows(
            new Markup($"[bold deepskyblue1]{Markup.Escape(heading)}[/]"),
            Text.NewLine,
            new Text(description),
            Text.NewLine,
            new Markup("[dim]Use the arrow keys to navigate and Enter to select.[/]"));

        return PanelFactory.Create(
            "INFORMATION",
            content,
            Color.DeepSkyBlue1);
    }

    private static (string Heading, string Description) GetInformation(
        MenuMode mode)
    {
        return mode switch
        {
            MenuMode.MainMenu => (
                "Calm.WeatherMonitor",
                "Select an option from the main menu to import weather data, " +
                "fetch live measurements or view stored statistics."),

            MenuMode.ImportFile => (
                "Import historical data",
                "Import and validate historical weather measurements from " +
                "a local data file."),

            MenuMode.ApiImport => (
                "Fetch live weather data",
                "Retrieve current weather measurements from the external " +
                "weather service and prepare them for storage."),

            MenuMode.Statistics => (
                "Weather statistics",
                "View stored measurements by weather station and inspect " +
                "minimum, maximum and average temperatures."),

            MenuMode.Exit => (
                "Exit application",
                "Close Calm.WeatherMonitor safely."),

            _ => (
                "Unknown screen",
                "No information is available for the current screen.")
        };
    }
}