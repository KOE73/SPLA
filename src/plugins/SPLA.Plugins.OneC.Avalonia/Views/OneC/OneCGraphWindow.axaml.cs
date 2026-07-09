using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SPLA.Plugins.OneC.Avalonia.Graph;
using SPLA.Plugins.OneC.Graph;
using SPLA.Plugins.OneC.Storage;

namespace SPLA.Plugins.OneC.Avalonia.Views.OneC;

public partial class OneCGraphWindow : Window
{
    private readonly string _databasePath;
    private readonly string _rootFullName;
    private readonly TextBlock _rootObjectText;
    private readonly ComboBox _modeSelector;
    private readonly ComboBox _depthSelector;
    private readonly ComboBox _limitSelector;
    private readonly Button _rebuildGraphButton;
    private readonly Button _toggleWindowModeButton;
    private readonly TextBlock _graphSummary;
    private readonly NativeWebView _graphView;

    public OneCGraphWindow()
    {
        _databasePath = "";
        _rootFullName = "";

        InitializeComponent();

        _rootObjectText = this.FindControl<TextBlock>("RootObjectText")!;
        _modeSelector = this.FindControl<ComboBox>("ModeSelector")!;
        _depthSelector = this.FindControl<ComboBox>("DepthSelector")!;
        _limitSelector = this.FindControl<ComboBox>("LimitSelector")!;
        _rebuildGraphButton = this.FindControl<Button>("RebuildGraphButton")!;
        _toggleWindowModeButton = this.FindControl<Button>("ToggleWindowModeButton")!;
        _graphSummary = this.FindControl<TextBlock>("GraphSummaryText")!;
        _graphView = this.FindControl<NativeWebView>("GraphWebView")!;
    }

    public OneCGraphWindow(string databasePath, string rootFullName, OneCGraphMode initialMode, int initialDepth = 3, int initialLimit = 400) : this()
    {
        _databasePath = databasePath;
        _rootFullName = rootFullName;

        Title = $"1C Graph: {rootFullName}";
        _rootObjectText.Text = rootFullName;

        _modeSelector.ItemsSource = Enum.GetValues<OneCGraphMode>();
        _modeSelector.SelectedItem = initialMode;
        _depthSelector.ItemsSource = new[] { 1, 2, 3, 4, 5 };
        _depthSelector.SelectedItem = Math.Clamp(initialDepth, 1, 5);
        _limitSelector.ItemsSource = new[] { 100, 200, 400, 800, 1200 };
        _limitSelector.SelectedItem = ClosestLimit(initialLimit);

        _rebuildGraphButton.Click += (_, _) => BuildGraph();
        _toggleWindowModeButton.Click += (_, _) => ToggleWindowMode();

        BuildGraph();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void BuildGraph()
    {
        try
        {
            using var db = new OneCIndexDatabase(_databasePath);
            var builder = new OneCGraphBuilder(db);
            var mode = _modeSelector.SelectedItem is OneCGraphMode selectedMode
                ? selectedMode
                : OneCGraphMode.Dependencies;
            var depth = _depthSelector.SelectedItem as int? ?? 3;
            var limit = _limitSelector.SelectedItem as int? ?? 400;
            var graph = builder.Build(new(_rootFullName, mode, depth, limit));

            _graphSummary.Text = $"""
                mode: {graph.Summary.Mode}
                depth: {graph.Summary.Depth}
                limit: {graph.Summary.Limit}
                nodes: {graph.Summary.NodeCount}
                edges: {graph.Summary.EdgeCount}
                truncated: {graph.Summary.Truncated}
                relations: {FormatRelationCounts(graph.Summary.RelationTypeCounts)}
                """;

            _graphView.NavigateToString(CytoscapeGraphAdapter.ToHtmlDocument(graph), new Uri("https://spla.local/onec-graph/"));

            if (graph.Summary.EdgeCount == 0)
            {
                _graphSummary.Text += "\nNo edges for this mode/selection. Try Dependencies or References, or increase depth.";
            }
        }
        catch (Exception ex)
        {
            _graphSummary.Text = $"Unable to build graph: {ex.Message}";
        }
    }

    private void ToggleWindowMode()
    {
        WindowState = WindowState == WindowState.FullScreen
            ? WindowState.Normal
            : WindowState.FullScreen;

        _toggleWindowModeButton.Content = WindowState == WindowState.FullScreen
            ? "Windowed"
            : "Full Screen";
    }

    private static string FormatRelationCounts(IReadOnlyDictionary<string, int> relationCounts) =>
        relationCounts.Count == 0
            ? "none"
            : string.Join(", ", relationCounts.OrderByDescending(pair => pair.Value).Select(pair => $"{pair.Key}={pair.Value}"));

    private static int ClosestLimit(int requestedLimit) =>
        new[] { 100, 200, 400, 800, 1200 }
            .OrderBy(value => Math.Abs(value - requestedLimit))
            .First();
}
