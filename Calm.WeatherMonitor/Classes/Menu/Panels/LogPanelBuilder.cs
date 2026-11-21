using Calm.WeatherMonitor.Classes.Menu.Factory;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Calm.WeatherMonitor.Classes.Menu.Panels;

public static class LogPanelBuilder
{
    private const int MaximumVisibleEntries = 6;

    public static Panel Build(MenuState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        IReadOnlyList<string> visibleEntries = state.LogEntries
            .TakeLast(MaximumVisibleEntries)
            .ToList();

        IRenderable content = visibleEntries.Count == 0
            ? CreateEmptyContent()
            : CreateLogContent(visibleEntries);

        return PanelFactory.Create(
            "ACTIVITY LOG",
            content,
            Color.Grey);
    }

    private static IRenderable CreateEmptyContent()
    {
        return new Markup("[dim]No activity has been recorded yet.[/]");
    }

    private static IRenderable CreateLogContent(
        IReadOnlyList<string> entries)
    {
        IEnumerable<IRenderable> rows = entries.Select(entry =>
            new Markup(
                $"[grey]>[/] {Markup.Escape(entry)}"));

        return new Rows(rows);
    }
}