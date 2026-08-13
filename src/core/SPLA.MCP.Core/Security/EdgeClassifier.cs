using System;
using System.Collections.Generic;
using System.Text.Json;
using SPLA.Domain.Models;
using SPLA.Domain.Security;
using SPLA.MCP.Core.Json;

namespace SPLA.MCP.Core.Security;

/// <summary>
/// Works out which movement a tool call actually is: out of where, into where.
///
/// <para><b>Derived, not declared.</b> There are 109 <c>Scope</c> declarations across this
/// repository and breaking every one of them to add two fields would be a migration for its own
/// sake. Almost all of them already imply the answer — a project read is
/// <c>project → context</c> — so the rule below reads them, and only the handful the rule gets
/// wrong are named explicitly. Those few are exactly the calls the single-axis model could not
/// express, which is why they are worth naming: <c>browser_upload</c> reads a file off the disk and
/// posts it to a web form, and calling that "an internet write" is how a leak passes for ordinary
/// work.</para>
///
/// <para>Islands are named by the argument that picks them (<c>connection</c>, <c>host</c>), so the
/// edge says <c>sql:prod</c> and not merely <c>sql</c>. Binding that name to the connection's
/// fingerprint is the grant's job, at the point where being wrong would matter.</para>
/// </summary>
public sealed class EdgeClassifier
{
    private readonly Func<string?, Zone> _zoneOfPath;

    /// <param name="zoneOfPath">Which side of the project boundary a path lands on. Injected because
    /// the boundary belongs to the host, and this class must not learn where the root is.</param>
    public EdgeClassifier(Func<string?, Zone> zoneOfPath) => _zoneOfPath = zoneOfPath;

    /// <summary>The tools whose generic answer is wrong, and what they really do.</summary>
    private static readonly Dictionary<string, Func<JsonElement, Func<string?, Zone>, ZoneEdge>> Named = new(StringComparer.OrdinalIgnoreCase)
    {
        // Declared Scope=Internet/Write, which reads as "posting something to the web" and hides the
        // half that matters: WHAT is being posted, and that it came off the disk.
        ["browser_upload"] = (args, zoneOf) => new ZoneEdge(
            zoneOf(FirstString(args, "files")), Zone.Web, ZoneEffect.Write),

        // Declared Local, i.e. as if a database on another machine were a file on this one.
        ["sql_query"]      = (a, _) => new ZoneEdge(SqlZone(a), Zone.Context, ZoneEffect.Read),
        ["sql_schema"]     = (a, _) => new ZoneEdge(SqlZone(a), Zone.Context, ZoneEffect.Read),
        ["sql_query_plan"] = (a, _) => new ZoneEdge(SqlZone(a), Zone.Context, ZoneEffect.Read),
        ["sql_execute"]    = (a, _) => new ZoneEdge(Zone.Context, SqlZone(a), ZoneEffect.Write),

        ["ssh_run"]          = (a, _) => new ZoneEdge(Zone.Context, SshZone(a), ZoneEffect.Execute),
        ["ssh_session_exec"] = (a, _) => new ZoneEdge(Zone.Context, SshZone(a), ZoneEffect.Execute),

        // The transfers, where source and sink are both real places and neither is the context.
        ["sftp_upload"] = (a, zoneOf) => new ZoneEdge(
            zoneOf(ToolJson.GetStringTrimmed(a, "local_path")), SshZone(a), ZoneEffect.Write),
        ["sftp_download"] = (a, zoneOf) => new ZoneEdge(
            SshZone(a), zoneOf(ToolJson.GetStringTrimmed(a, "local_path")), ZoneEffect.Write)
    };

    /// <summary>
    /// The edge this call is. Falls back to <see cref="Zone.Unknown"/> rather than to a guess: an
    /// invented source becomes an invented verdict, and a verdict nobody can account for is worse
    /// than none.
    /// </summary>
    public ZoneEdge Classify(ToolFunctionDefinition tool, string? argumentsJson)
    {
        JsonElement args = default;
        var haveArgs = false;
        if (!string.IsNullOrWhiteSpace(argumentsJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(argumentsJson);
                args = doc.RootElement.Clone();
                haveArgs = true;
            }
            catch (JsonException) { /* an unparseable call is classified on its declaration alone */ }
        }

        if (haveArgs && Named.TryGetValue(tool.Name, out var named))
            return named(args, _zoneOfPath);

        var path = haveArgs ? ToolJson.GetStringTrimmed(args, "path") : null;

        return tool.Scope switch
        {
            // Not a place. A shell reaches everything, and saying so plainly is what makes it obvious
            // that no rule here bounds it — only an isolator does.
            ToolScope.Shell => new ZoneEdge(Zone.Any, Zone.Any, ZoneEffect.Execute),

            // The agent's own state. Never gated, and an edge here would only add noise.
            ToolScope.Agent or ToolScope.Skill => new ZoneEdge(Zone.Agent, Zone.Agent, Effect(tool)),

            ToolScope.Internet => tool.Effect == ToolEffect.Read
                ? new ZoneEdge(Zone.Web, Zone.Context, ZoneEffect.Read)
                : new ZoneEdge(Zone.Context, Zone.Web, ZoneEffect.Write),

            // Files. Which side of the boundary decides the zone; the declaration only says whether
            // content is coming out or going in.
            _ => tool.Effect == ToolEffect.Read
                ? new ZoneEdge(_zoneOfPath(path), Zone.Context, ZoneEffect.Read)
                : new ZoneEdge(Zone.Context, _zoneOfPath(path), ZoneEffect.Write)
        };
    }

    private static ZoneEffect Effect(ToolFunctionDefinition tool) => tool.Effect switch
    {
        ToolEffect.Read => ZoneEffect.Read,
        ToolEffect.Execute => ZoneEffect.Execute,
        _ => ZoneEffect.Write
    };

    /// <summary>The database this call names, or the project's default when it names none. "default"
    /// is a real instance — it is one particular connection — not a missing answer.</summary>
    private static Zone SqlZone(JsonElement args)
        => new("sql", ToolJson.GetStringTrimmed(args, "connection") ?? "default");

    private static Zone SshZone(JsonElement args)
        => new("ssh", ToolJson.GetStringTrimmed(args, "host") ?? "default");

    /// <summary>First element of a string-or-array argument. Upload takes a list; the zone of the
    /// first file is enough to say which side of the boundary the batch came from.</summary>
    private static string? FirstString(JsonElement args, string name)
    {
        if (!args.TryGetProperty(name, out var value)) return null;

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Array when value.GetArrayLength() > 0 => value[0].GetString(),
            _ => null
        };
    }
}
