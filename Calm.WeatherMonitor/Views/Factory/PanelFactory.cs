using Spectre.Console;
using Spectre.Console.Rendering;

namespace Calm.WeatherMonitor.App.Views.Factory;

public static class PanelFactory
{
    public static Panel Create(
        string title,
        IRenderable content,
        Color? borderColor = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(content);

        return new Panel(content)
            .Header($"[bold]{Markup.Escape(title)}[/]", Justify.Left)
            .Border(BoxBorder.Rounded)
            .BorderColor(borderColor ?? Color.Grey)
            .Padding(1, 0)
            .Expand();
    }
}