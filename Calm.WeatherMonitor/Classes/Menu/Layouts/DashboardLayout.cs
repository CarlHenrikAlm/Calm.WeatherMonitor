using Calm.WeatherMonitor.Classes.Menu.Panels;
using Spectre.Console;

namespace Calm.WeatherMonitor.Classes.Menu.Layouts;

public static class DashboardLayout
{
    public static Layout Build(MenuState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        Layout layout = CreateLayout();

        UpdatePanels(layout, state);

        return layout;
    }

    private static Layout CreateLayout()
    {
        var layout = new Layout("Root");

        layout.SplitRows(
            new Layout("Workspace")
                .Ratio(3)
                .SplitColumns(
                    new Layout("Sidebar")
                        .Ratio(1)
                        .SplitRows(
                            new Layout("Menu")
                                .Ratio(3),
                            new Layout("Status")
                                .Ratio(1)),
                    new Layout("Information")
                        .Ratio(3)),
            new Layout("Log")
                .Ratio(1));

        return layout;
    }

    private static void UpdatePanels(
        Layout layout,
        MenuState state)
    {
        layout["Menu"].Update(
            MenuPanelBuilder.Build(state));

        layout["Status"].Update(
            StatusPanelBuilder.Build(state));

        layout["Information"].Update(
            InformationPanelBuilder.Build(state));

        layout["Log"].Update(
            LogPanelBuilder.Build(state));
    }
}