using SPLA.Domain.Settings;
using SPLA.MCP.Core.Interfaces;
using SPLA.Plugins.Geometry.Tools;
using System.Collections.Generic;

namespace SPLA.Plugins.Geometry;

/// <summary>
/// Interactive geometry workspace: the model places a rough box on an image, sees it drawn, and
/// corrects it — the tool owns the exact coordinates, the transforms and the rendering.
/// See <c>docs/adr/ADR_20260914_plugins_geometry-workspace.md</c>.
/// <para>
/// Like <c>BrowserPlugin</c>, every tool resolves its markup session lazily from the ambient chat
/// (<c>GeometrySessionRegistry</c>): <see cref="Initialize"/> runs once per process,
/// not once per chat, so tool instances are shared and per-chat isolation happens at call time.
/// </para>
/// </summary>
public sealed class GeometryPlugin : ISplaPlugin
{
    /// <summary>Captured for the tools: image addresses are resolved through
    /// <c>ResourceRegistry.For(settings)</c>, which needs the project's resolved settings.</summary>
    internal ResolvedSettings Settings { get; private set; } = new();

    public IEnumerable<IMcpTool> Initialize(ResolvedSettings settings)
    {
        Settings = settings;
        return [new GeometryOpenTool(settings)];
    }
}
