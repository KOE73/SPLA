using System.Text.Json;

namespace SPLA.Plugins.OneC.Graph;

/// <summary>
/// Renders an <see cref="OneCGraph"/> into the Cytoscape.js shapes the graph view expects.
///
/// Two outputs:
///   * <see cref="ToJson"/>       — the { nodes, edges } payload consumed by the web panel
///                                  (passed to <c>window.loadGraph(json)</c> in onec_graph.html).
///   * <see cref="ToHtmlDocument"/> — a fully self-contained HTML page that renders the graph
///                                  without any host bridge. Handy for a quick "open graph in a
///                                  new tab" flow or offline export.
///
/// This type is UI-framework independent (no Avalonia/WPF), so it lives in the plugin proper and is
/// shared by every host (CLI, service/web). It used to live in SPLA.Plugins.OneC.Avalonia.
/// </summary>
public static class CytoscapeGraphAdapter
{
    public static string ToJson(OneCGraph graph)
    {
        var payload = new
        {
            nodes = graph.Nodes.Select(node => new
            {
                id = node.Id,
                label = node.Label,
                kind = node.Kind,
                isCenter = node.IsCenter
            }),
            edges = graph.Edges.Select(edge => new
            {
                id = edge.Id,
                source = edge.SourceId,
                target = edge.TargetId,
                type = edge.RelationType
            })
        };

        return JsonSerializer.Serialize(payload);
    }

    public static string ToHtmlDocument(OneCGraph graph) =>
        $$$"""
        <!DOCTYPE html>
        <html>
        <head>
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <style>
          :root {
            --bg: #1e1e1e;
            --panel: #252526;
            --border: #333333;
            --text: #ffffff;
            --subtext: #888888;
            --accent: #007acc;
            --green: #4caf50;
            --red: #d35f5f;
            --yellow: #d7b56d;
            --orange: #d98b4a;
            --mauve: #aa78d6;
            --teal: #4fb7b5;
          }
          html, body { margin: 0; width: 100%; height: 100%; overflow: hidden; background: var(--bg); color: var(--text); font-family: Segoe UI, sans-serif; }
          #toolbar { height: 34px; display: flex; align-items: center; gap: 8px; padding: 0 10px; background: var(--panel); border-bottom: 1px solid var(--border); font-size: 12px; }
          #root { color: var(--accent); font-weight: 600; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
          #summary { color: var(--subtext); margin-left: auto; }
          #cy { width: 100%; height: calc(100% - 35px); }
        </style>
        </head>
        <body>
        <div id="toolbar">
          <span id="root">{{{Escape(graph.Summary.RootFullName)}}}</span>
          <span id="summary">{{{graph.Summary.Mode}}} · nodes {{{graph.Summary.NodeCount}}} · edges {{{graph.Summary.EdgeCount}}}</span>
        </div>
        <div id="cy"></div>
        <script src="https://cdnjs.cloudflare.com/ajax/libs/cytoscape/3.30.2/cytoscape.min.js"></script>
        <script>
        const graph = {{{ToJson(graph)}}};
        const kindColor = kind => ({
          Document: '#89b4fa',
          Catalog: '#a6e3a1',
          AccumulationRegister: '#f38ba8',
          InformationRegister: '#fab387',
          AccountingRegister: '#f38ba8',
          CalculationRegister: '#f38ba8',
          CommonModule: '#cba6f7',
          Report: '#f9e2af',
          Processing: '#f9e2af',
          Form: '#94e2d5',
          TabularSection: '#94e2d5'
        }[kind] || '#6c7086');
        const edgeColor = type => ({
          writes: '#f38ba8',
          reads: '#89b4fa',
          queries: '#fab387',
          calls: '#a6e3a1',
          owns: '#6c7086',
          uses: '#cba6f7'
        }[type] || '#6c7086');
        const elements = [];
        graph.nodes.forEach(n => elements.push({ data: n }));
        graph.edges.forEach(e => elements.push({ data: e }));
        const cy = cytoscape({
          container: document.getElementById('cy'),
          elements,
          wheelSensitivity: 0.25,
          style: [
            { selector: 'node', style: {
              'background-color': n => kindColor(n.data('kind')),
              'label': 'data(label)',
              'color': '#ffffff',
              'font-size': '10px',
              'text-valign': 'bottom',
              'text-margin-y': 4,
              'text-outline-width': 2,
              'text-outline-color': '#1e1e1e',
              'width': 34,
              'height': 34
            }},
            { selector: 'node[?isCenter]', style: {
              'width': 52,
              'height': 52,
              'border-width': 3,
              'border-color': '#ffffff',
              'font-size': '12px',
              'font-weight': 'bold'
            }},
            { selector: 'edge', style: {
              'line-color': e => edgeColor(e.data('type')),
              'target-arrow-color': e => edgeColor(e.data('type')),
              'target-arrow-shape': 'triangle',
              'curve-style': 'bezier',
              'width': 1.6,
              'opacity': 0.85,
              'label': 'data(type)',
              'font-size': 8,
              'color': '#888888',
              'text-rotation': 'autorotate',
              'text-background-color': '#1e1e1e',
              'text-background-opacity': 0.7
            }}
          ],
          layout: { name: 'cose', animate: false, padding: 35, nodeRepulsion: 6500, idealEdgeLength: 120 }
        });
        setTimeout(() => cy.fit(undefined, 30), 100);
        </script>
        </body>
        </html>
        """;

    private static string Escape(string value) =>
        value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
