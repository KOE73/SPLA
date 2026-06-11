using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using SPLA.Plugins.Host.Avalonia;
using SPLA.Plugins.OneC.Avalonia.Context;
using SPLA.Plugins.OneC.Graph;
using SPLA.Plugins.OneC.Indexing;
using SPLA.Plugins.OneC.Models;
using SPLA.Plugins.OneC.Storage;

namespace SPLA.Plugins.OneC.Avalonia.Views.OneC;

public partial class OneCOverviewPanel : UserControl
{
    private readonly AvaloniaPluginPanelContext? _context;
    private readonly string _databasePath;
    private readonly TextBox _searchBox;
    private readonly TreeView _tree;
    private readonly ListBox _searchResults;
    private readonly TextBlock _objectTitle;
    private readonly TextBlock _objectDetails;
    private readonly TextBlock _graphSummary;
    private readonly NativeWebView _graphView;
    private readonly TextBlock _statusText;
    private readonly TextBlock _indexStatus;
    private readonly TextBlock _objectsValue;
    private readonly TextBlock _relationsValue;
    private readonly TextBlock _sectionsValue;
    private readonly ComboBox _formatSelector;
    private readonly Button _rebuildIndexButton;
    private readonly Button _copyButton;
    private readonly Button _insertButton;
    private readonly Button _dependenciesButton;
    private readonly Button _referencesButton;
    private readonly Button _dataFlowButton;
    private readonly List<IOneCContextFormatter> _formatters =
    [
        new FullNameContextFormatter(),
        new ObjectCardYamlContextFormatter(),
        new GraphSummaryContextFormatter(),
        new SelectedRelationsContextFormatter()
    ];
    private List<OneCObject> _objects = [];
    private Dictionary<string, OneCObject> _objectsByFullName = new(StringComparer.OrdinalIgnoreCase);
    private OneCGraph? _selectedGraph;

    public OneCOverviewPanel()
    {
        _databasePath = "";
        InitializeComponent();

        _searchBox = this.FindControl<TextBox>("SearchBox")!;
        _tree = this.FindControl<TreeView>("ObjectsTree")!;
        _searchResults = this.FindControl<ListBox>("SearchResultsList")!;
        _objectTitle = this.FindControl<TextBlock>("ObjectTitleText")!;
        _objectDetails = this.FindControl<TextBlock>("ObjectDetailsText")!;
        _graphSummary = this.FindControl<TextBlock>("GraphSummaryText")!;
        _graphView = this.FindControl<NativeWebView>("GraphWebView")!;
        _statusText = this.FindControl<TextBlock>("StatusTextBlock")!;
        _indexStatus = this.FindControl<TextBlock>("IndexStatusTextBlock")!;
        _objectsValue = this.FindControl<TextBlock>("ObjectsValueText")!;
        _relationsValue = this.FindControl<TextBlock>("RelationsValueText")!;
        _sectionsValue = this.FindControl<TextBlock>("SectionsValueText")!;
        _formatSelector = this.FindControl<ComboBox>("FormatSelector")!;
        _rebuildIndexButton = this.FindControl<Button>("RebuildIndexButton")!;
        _copyButton = this.FindControl<Button>("CopyButton")!;
        _insertButton = this.FindControl<Button>("InsertButton")!;
        _dependenciesButton = this.FindControl<Button>("DependenciesButton")!;
        _referencesButton = this.FindControl<Button>("ReferencesButton")!;
        _dataFlowButton = this.FindControl<Button>("DataFlowButton")!;

        WireEvents();
    }

    public OneCOverviewPanel(AvaloniaPluginPanelContext context) : this()
    {
        _context = context;
        _databasePath = Path.Combine(context.WorkspacePath, ".spla", "onec.sqlite");
        LoadOverview();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void WireEvents()
    {
        _tree.SelectionChanged += (_, _) => SelectTreeObject();
        _searchResults.SelectionChanged += (_, _) => SelectSearchObject();
        _searchResults.DisplayMemberBinding = new global::Avalonia.Data.Binding(nameof(OneCObject.FullName));
        _searchBox.TextChanged += (_, _) => ApplySearch(_searchBox.Text ?? "");

        _formatSelector.ItemsSource = _formatters;
        _formatSelector.DisplayMemberBinding = new global::Avalonia.Data.Binding(nameof(IOneCContextFormatter.DisplayName));
        _formatSelector.SelectedIndex = 0;

        _rebuildIndexButton.Click += async (_, _) => await RebuildIndexAsync(_rebuildIndexButton);
        _copyButton.Click += async (_, _) =>
        {
            var text = FormatSelection();
            if (!string.IsNullOrWhiteSpace(text))
            {
                await _context!.Interaction.CopyToClipboardAsync(text);
            }
        };
        _insertButton.Click += (_, _) =>
        {
            var text = FormatSelection();
            if (!string.IsNullOrWhiteSpace(text))
            {
                _context!.Interaction.InsertIntoPrompt(text);
            }
        };
        _dependenciesButton.Click += (_, _) => BuildSelectedGraph(OneCGraphMode.Dependencies);
        _referencesButton.Click += (_, _) => BuildSelectedGraph(OneCGraphMode.References);
        _dataFlowButton.Click += (_, _) => BuildSelectedGraph(OneCGraphMode.DataFlow);
    }

    private void LoadOverview()
    {
        try
        {
            using var db = new OneCIndexDatabase(_databasePath);
            db.EnsureCreated();
            _objects = db.GetAllObjects();
            _objectsByFullName = _objects.ToDictionary(o => o.FullName, StringComparer.OrdinalIgnoreCase);
            var owns = db.GetRelationsByType(RelationType.Owns);
            var kindCounts = _objects.GroupBy(o => o.Kind).ToDictionary(g => g.Key, g => g.Count());

            _statusText.Text = _databasePath;
            _objectsValue.Text = _objects.Count.ToString();
            _relationsValue.Text = db.CountRelations().ToString();
            _sectionsValue.Text = kindCounts.Count.ToString();
            _tree.ItemsSource = BuildTree(owns);
            ApplySearch("");
        }
        catch (Exception ex)
        {
            _statusText.Text = $"Unable to open OneC index: {ex.Message}";
        }
    }

    private async Task RebuildIndexAsync(Button trigger)
    {
        trigger.IsEnabled = false;
        _indexStatus.Text = "Indexing started...";

        try
        {
            using var db = new OneCIndexDatabase(_databasePath);
            var indexer = new OneCIndexer(db);
            var progress = new Progress<string>(message => _indexStatus.Text = message);
            var report = await Task.Run(() => indexer.Index(_context!.WorkspacePath, progress));

            _indexStatus.Text = $"""
                Indexing complete.
                Objects: +{report.ObjectsAdded} ~{report.ObjectsUpdated}
                Relations: +{report.RelationsAdded}
                Skipped: {report.FilesSkipped}
                Errors: {report.FilesWithErrors}
                Elapsed: {report.Elapsed.TotalSeconds:F1}s
                """;
            LoadOverview();
        }
        catch (Exception ex)
        {
            _indexStatus.Text = $"Indexing failed: {ex.Message}";
        }
        finally
        {
            trigger.IsEnabled = true;
        }
    }

    private List<TreeViewItem> BuildTree(List<RelationRow> owns)
    {
        var childNames = owns.Select(r => r.ToFullName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var childrenByParent = owns
            .GroupBy(r => r.FromFullName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        return _objects
            .Where(o => !childNames.Contains(o.FullName) && IsTopLevelObject(o))
            .GroupBy(o => o.Kind)
            .OrderBy(g => SectionOrder(g.Key))
            .ThenBy(g => SectionTitle(g.Key))
            .Select(group => new TreeViewItem
            {
                Header = $"{SectionTitle(group.Key)} ({group.Count()})",
                ItemsSource = group
                    .OrderBy(o => o.Name)
                    .Select(o => ObjectNode(o, childrenByParent))
                    .ToList()
            })
            .ToList();
    }

    private TreeViewItem ObjectNode(OneCObject obj, Dictionary<string, List<RelationRow>> childrenByParent) =>
        new()
        {
            Header = $"{obj.Name}  [{obj.Kind}]",
            Tag = obj,
            ItemsSource = childrenByParent.TryGetValue(obj.FullName, out var children)
                ? children
                    .Where(r => _objectsByFullName.ContainsKey(r.ToFullName))
                    .Select(r => ObjectNode(_objectsByFullName[r.ToFullName], childrenByParent))
                    .ToList()
                : []
        };

    private void ApplySearch(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            _searchResults.ItemsSource = null;
            _searchResults.IsVisible = false;
            return;
        }

        _searchResults.IsVisible = true;
        _searchResults.ItemsSource = _objects
            .Where(o => o.FullName.Contains(query, StringComparison.OrdinalIgnoreCase)
                || o.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(80)
            .ToList();
    }

    private void SelectTreeObject()
    {
        if (_tree.SelectedItem is TreeViewItem { Tag: OneCObject obj })
        {
            ShowObject(obj);
        }
    }

    private void SelectSearchObject()
    {
        if (_searchResults.SelectedItem is OneCObject obj)
        {
            ShowObject(obj);
        }
    }

    private void ShowObject(OneCObject obj)
    {
        _selectedGraph = null;
        _objectTitle.Text = obj.FullName;
        _objectDetails.Text = $"""
            kind: {obj.Kind}
            name: {obj.Name}
            fullName: {obj.FullName}
            path: {obj.Path ?? ""}
            summary: {obj.Summary ?? ""}

            Actions:
            - Copy/Insert работает по выбранному формату справа от граф-кнопок.
            - Graph buttons render the graph below in this panel.
            """;
        _graphSummary.Text = "Select Dependencies Graph, References Graph, or Data Flow Graph.";
        NavigateGraphView(EmptyGraphHtml());
    }

    private void BuildSelectedGraph(OneCGraphMode mode)
    {
        if (SelectedObject is not { } selected)
        {
            _graphSummary.Text = "Select an object first.";
            return;
        }

        try
        {
            using var db = new OneCIndexDatabase(_databasePath);
            var builder = new OneCGraphBuilder(db);
            _selectedGraph = builder.Build(new(selected.FullName, mode, Depth: 3, Limit: 400));
            _graphSummary.Text = $"""
                mode: {mode}
                nodes: {_selectedGraph.Summary.NodeCount}
                edges: {_selectedGraph.Summary.EdgeCount}
                depth: {_selectedGraph.Summary.Depth}
                limit: {_selectedGraph.Summary.Limit}
                truncated: {_selectedGraph.Summary.Truncated}
                """;
            NavigateGraphView(Graph.CytoscapeGraphAdapter.ToHtmlDocument(_selectedGraph));
            _indexStatus.Text = $"Rendered {mode} graph: nodes={_selectedGraph.Summary.NodeCount}, edges={_selectedGraph.Summary.EdgeCount}";
        }
        catch (Exception ex)
        {
            _graphSummary.Text = $"Unable to build graph: {ex.Message}";
        }
    }

    private void NavigateGraphView(string html)
    {
        try
        {
            _graphView.NavigateToString(html, new Uri("https://spla.local/onec-graph/"));
        }
        catch (Exception ex)
        {
            _graphSummary.Text = $"{_graphSummary.Text}\nWebView unavailable: {ex.Message}".Trim();
        }
    }

    private static string EmptyGraphHtml() =>
        """
        <!DOCTYPE html>
        <html>
        <body style="margin:0;height:100vh;display:flex;align-items:center;justify-content:center;background:#1e1e1e;color:#888888;font:14px Segoe UI,sans-serif;">
        Select graph mode to render dependencies.
        </body>
        </html>
        """;

    private string FormatSelection() =>
        _formatSelector.SelectedItem is IOneCContextFormatter formatter
            ? formatter.Format(new(SelectedObject, _selectedGraph))
            : "";

    private OneCObject? SelectedObject =>
        _tree.SelectedItem is TreeViewItem { Tag: OneCObject treeObj }
            ? treeObj
            : _searchResults.SelectedItem as OneCObject;

    private static bool IsTopLevelObject(OneCObject obj) =>
        obj.FullName.Count(c => c == '.') == 1;

    private static int SectionOrder(string kind) => kind switch
    {
        "Catalog" => 10,
        "Document" => 20,
        "CommonModule" => 30,
        "AccumulationRegister" => 40,
        "InformationRegister" => 50,
        "AccountingRegister" => 60,
        "Report" => 70,
        "Processing" => 80,
        "BusinessProcess" => 90,
        "Task" => 100,
        _ => 500
    };

    private static string SectionTitle(string kind) => kind switch
    {
        "Catalog" => "Справочники",
        "Document" => "Документы",
        "CommonModule" => "Общие модули",
        "AccumulationRegister" => "Регистры накопления",
        "InformationRegister" => "Регистры сведений",
        "AccountingRegister" => "Регистры бухгалтерии",
        "Report" => "Отчеты",
        "Processing" => "Обработки",
        "BusinessProcess" => "Бизнес-процессы",
        "Task" => "Задачи",
        _ => kind
    };
}
