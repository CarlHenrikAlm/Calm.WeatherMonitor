using Calm.WeatherMonitor.Classes.Menu.Factory;
using Calm.WeatherMonitor.Classes.Menu.States;
using Calm.WeatherMonitor.Models.Results;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Calm.WeatherMonitor.Classes.Menu.Panels;

public static class InformationPanelBuilder
{
    private const int MeasurementsPerPage = 8;

    public static Panel Build(MenuState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        IRenderable content = state.Mode switch
        {
            MenuMode.Statistics
                when state.IsSelectingStation =>
                    BuildStationSelectionContent(state),

            MenuMode.Statistics
                when state.StatisticsResult is not null =>
                    BuildStatisticsContent(
                        state.StatisticsResult,
                        state.StatisticsPageIndex),

            _ =>
                BuildDefaultContent(state)
        };

        return PanelFactory.Create(
            "INFORMATION",
            content,
            Color.DeepSkyBlue1);
    }

    private static IRenderable BuildDefaultContent(
        MenuState state)
    {
        (string heading, string defaultDescription) =
            GetInformation(state.Mode);

        string description =
            string.IsNullOrWhiteSpace(
                state.InformationMessage)
                    ? defaultDescription
                    : state.InformationMessage;

        return new Rows(
            new Markup(
                $"[bold deepskyblue1]" +
                $"{Markup.Escape(heading)}[/]"),

            Text.NewLine,

            new Text(description),

            Text.NewLine,

            new Markup(
                "[dim]Use the arrow keys to navigate " +
                "and Enter to select.[/]"));
    }

    private static IRenderable BuildStationSelectionContent(
    MenuState state)
    {
        if (state.StationNames.Count == 0)
        {
            return new Rows(
                new Markup(
                    "[bold deepskyblue1]" +
                    "Select weather station[/]"),

                Text.NewLine,

                new Markup(
                    "[yellow]No weather stations " +
                    "are available.[/]"));
        }

        var rows = new List<IRenderable>
    {
        new Markup(
            "[bold deepskyblue1]" +
            "Select weather station[/]"),

        Text.NewLine
    };

        for (
            int index = 0;
            index < state.StationNames.Count;
            index++)
        {
            string stationName =
                Markup.Escape(
                    state.StationNames[index]);

            bool isSelected =
                index == state.SelectedStationIndex;

            string row = isSelected
                ? $"[black on deepskyblue1]> {stationName}[/]"
                : $"[grey]  {stationName}[/]";

            rows.Add(new Markup(row));
        }

        rows.Add(Text.NewLine);

        rows.Add(
            new Markup(
                "[dim]Use Up/Down to select a station. " +
                "Press Enter to show statistics. " +
                "Press Escape to return.[/]"));

        return new Rows(rows);
    }

    private static IRenderable BuildStatisticsContent(
        StationStatisticsResult statistics,
        int requestedPageIndex)
    {
        int pageCount = CalculatePageCount(
            statistics.MeasurementCount);

        int pageIndex = Math.Clamp(
            requestedPageIndex,
            0,
            pageCount - 1);

        Table summaryTable =
            BuildSummaryTable(statistics);

        Table measurementTable =
            BuildMeasurementTable(
                statistics,
                pageIndex);

        return new Rows(
            new Markup(
                $"[bold deepskyblue1]" +
                $"{Markup.Escape(statistics.StationName)}[/]"),

            Text.NewLine,

            summaryTable,

            Text.NewLine,

            measurementTable,

            Text.NewLine,

            new Markup(
                $"[dim]Page {pageIndex + 1} " +
                $"of {pageCount}. " +
                "Use Left/Right to change page. " +
                "Press Enter to select another station.[/]"));
    }

    private static Table BuildSummaryTable(
        StationStatisticsResult statistics)
    {
        var table = new Table()
            .RoundedBorder()
            .BorderColor(Color.Grey)
            .Expand();

        table.AddColumn(
            new TableColumn("[grey]Measurements[/]")
                .Centered());

        table.AddColumn(
            new TableColumn("[blue]Minimum[/]")
                .RightAligned());

        table.AddColumn(
            new TableColumn("[red]Maximum[/]")
                .RightAligned());

        table.AddColumn(
            new TableColumn("[green]Average[/]")
                .RightAligned());

        table.AddRow(
            statistics.MeasurementCount.ToString(),
            $"{statistics.MinimumTemperature:F1} °C",
            $"{statistics.MaximumTemperature:F1} °C",
            $"{statistics.AverageTemperature:F1} °C");

        return table;
    }

    private static Table BuildMeasurementTable(
        StationStatisticsResult statistics,
        int pageIndex)
    {
        var table = new Table()
            .RoundedBorder()
            .BorderColor(Color.DeepSkyBlue1)
            .Expand();

        table.AddColumn(
            new TableColumn("[bold]Timestamp[/]")
                .NoWrap());

        table.AddColumn(
            new TableColumn("[bold]Temperature[/]")
                .RightAligned());

        table.AddColumn(
            new TableColumn("[bold]Humidity[/]")
                .RightAligned());

        table.AddColumn(
            new TableColumn("[bold]Source[/]")
                .Centered());

        IEnumerable<WeatherMeasurementResult> page =
            statistics.Measurements
                .Skip(
                    pageIndex * MeasurementsPerPage)
                .Take(MeasurementsPerPage);

        foreach (
            WeatherMeasurementResult measurement
            in page)
        {
            table.AddRow(
                measurement.Timestamp.ToString(
                    "yyyy-MM-dd HH:mm"),

                $"{measurement.Temperature:F1} °C",

                $"{measurement.Humidity:F1} %",

                Markup.Escape(
                    measurement.DataSource));
        }

        return table;
    }

    private static int CalculatePageCount(
        int measurementCount)
    {
        return Math.Max(
            1,
            (int)Math.Ceiling(
                measurementCount
                / (double)MeasurementsPerPage));
    }

    private static (
        string Heading,
        string Description)
        GetInformation(MenuMode mode)
    {
        return mode switch
        {
            MenuMode.MainMenu => (
                "Calm WeatherMonitor",
                "Select an option from the main menu " +
                "to import weather data, fetch live " +
                "measurements or view stored statistics."),

            MenuMode.ImportFile => (
                "Import historical data",
                "Import and validate historical weather " +
                "measurements from a local data file."),

            MenuMode.ApiImport => (
                "Fetch live weather data",
                "Retrieve current weather measurements " +
                "from Open-Meteo and save them."),

            MenuMode.Statistics => (
                "Weather statistics",
                "Select a weather station to view its " +
                "measurements and temperature statistics."),

            MenuMode.Exit => (
                "Exit application",
                "Close Calm WeatherMonitor safely."),

            _ => (
                "Unknown screen",
                "No information is available for the " +
                "current screen.")
        };
    }
}