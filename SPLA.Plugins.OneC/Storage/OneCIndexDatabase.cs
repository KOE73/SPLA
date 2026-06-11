using Microsoft.Data.Sqlite;
using SPLA.Plugins.OneC.Models;

namespace SPLA.Plugins.OneC.Storage;

/// <summary>
/// Low-level data access layer over the onec.sqlite index.
/// Thread-safety: use one instance per logical operation (not shared across threads).
/// </summary>
public class OneCIndexDatabase : IDisposable
{
    private readonly SqliteConnection _conn;
    private bool _disposed;

    public OneCIndexDatabase(string databasePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
        _conn = new SqliteConnection($"Data Source={databasePath}");
        _conn.Open();

        // Enable WAL for better concurrent read performance
        using var wal = _conn.CreateCommand();
        wal.CommandText = "PRAGMA journal_mode=WAL; PRAGMA foreign_keys=ON;";
        wal.ExecuteNonQuery();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Schema
    // ─────────────────────────────────────────────────────────────────────────

    public void EnsureCreated()
    {
        foreach (var ddl in OneCIndexSchema.All)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = ddl;
            cmd.ExecuteNonQuery();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Objects
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Insert or update an object by FullName.
    /// Returns the row Id.
    /// </summary>
    public long UpsertObject(OneCObject obj)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Objects (Kind, Name, FullName, Path, Summary)
            VALUES ($kind, $name, $fullName, $path, $summary)
            ON CONFLICT(FullName) DO UPDATE SET
                Kind    = excluded.Kind,
                Name    = excluded.Name,
                Path    = excluded.Path,
                Summary = excluded.Summary
            RETURNING Id;
            """;
        cmd.Parameters.AddWithValue("$kind",     obj.Kind);
        cmd.Parameters.AddWithValue("$name",     obj.Name);
        cmd.Parameters.AddWithValue("$fullName", obj.FullName);
        cmd.Parameters.AddWithValue("$path",     (object?)obj.Path    ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$summary",  (object?)obj.Summary ?? DBNull.Value);
        return (long)cmd.ExecuteScalar()!;
    }

    public long? GetObjectId(string fullName)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT Id FROM Objects WHERE FullName = $fn;";
        cmd.Parameters.AddWithValue("$fn", fullName);
        var result = cmd.ExecuteScalar();
        return result is null ? null : (long)result;
    }

    /// <summary>Find objects whose FullName or Name contains the query string (case-insensitive).</summary>
    public List<OneCObject> FindObjects(string query, int limit = 20, int offset = 0)
    {
        using var cmd = _conn.CreateCommand();
        var pattern = $"%{query}%";
        cmd.CommandText = """
            SELECT Id, Kind, Name, FullName, Path, Summary
            FROM Objects
            WHERE FullName LIKE $p OR Name LIKE $p
            ORDER BY Kind, Name
            LIMIT $limit OFFSET $offset;
            """;
        cmd.Parameters.AddWithValue("$p",      pattern);
        cmd.Parameters.AddWithValue("$limit",  limit);
        cmd.Parameters.AddWithValue("$offset", offset);
        return ReadObjects(cmd);
    }

    public OneCObject? GetObjectByFullName(string fullName)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = """
            SELECT Id, Kind, Name, FullName, Path, Summary
            FROM Objects WHERE FullName = $fn;
            """;
        cmd.Parameters.AddWithValue("$fn", fullName);
        return ReadObjects(cmd).FirstOrDefault();
    }

    public List<OneCObject> GetObjectsByKind(string kind, int limit = 200, int offset = 0)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = """
            SELECT Id, Kind, Name, FullName, Path, Summary
            FROM Objects WHERE Kind = $kind
            ORDER BY Name
            LIMIT $limit OFFSET $offset;
            """;
        cmd.Parameters.AddWithValue("$kind",   kind);
        cmd.Parameters.AddWithValue("$limit",  limit);
        cmd.Parameters.AddWithValue("$offset", offset);
        return ReadObjects(cmd);
    }

    public List<OneCObject> GetAllObjects(int limit = 20000)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = """
            SELECT Id, Kind, Name, FullName, Path, Summary
            FROM Objects
            ORDER BY Kind, Name
            LIMIT $limit;
            """;
        cmd.Parameters.AddWithValue("$limit", limit);
        return ReadObjects(cmd);
    }

    public int CountObjects()
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Objects;";
        return (int)(long)cmd.ExecuteScalar()!;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Relations
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Insert a relation, silently ignoring duplicates
    /// (same FromObjectId, ToObjectId, Type, SourcePath, SourceLine).
    /// </summary>
    public void UpsertRelation(OneCRelation rel)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = """
            INSERT OR IGNORE INTO Relations
                (FromObjectId, ToObjectId, Type, SourcePath, SourceLine, Confidence)
            VALUES
                ($from, $to, $type, $srcPath, $srcLine, $conf);
            """;
        cmd.Parameters.AddWithValue("$from",    rel.FromObjectId);
        cmd.Parameters.AddWithValue("$to",      rel.ToObjectId);
        cmd.Parameters.AddWithValue("$type",    rel.Type);
        cmd.Parameters.AddWithValue("$srcPath", (object?)rel.SourcePath  ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$srcLine", (object?)rel.SourceLine  ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$conf",    rel.Confidence);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Relations going OUT from an object (its dependencies).</summary>
    public List<RelationRow> GetRelationsFrom(long fromId, string[]? types = null, int limit = 500)
    {
        return QueryRelations("FromObjectId", fromId, types, limit);
    }

    /// <summary>Relations coming IN to an object (who uses it).</summary>
    public List<RelationRow> GetRelationsTo(long toId, string[]? types = null, int limit = 500)
    {
        return QueryRelations("ToObjectId", toId, types, limit);
    }

    public int CountRelationsTo(long toId, string[]? types = null)
    {
        using var cmd = _conn.CreateCommand();
        var typeFilter = BuildTypeFilter(types, out var typeParams);
        cmd.CommandText = $"SELECT COUNT(*) FROM Relations WHERE ToObjectId = $id{typeFilter};";
        cmd.Parameters.AddWithValue("$id", toId);
        AddTypeParams(cmd, typeParams);
        return (int)(long)cmd.ExecuteScalar()!;
    }

    public int CountRelations()
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Relations;";
        return (int)(long)cmd.ExecuteScalar()!;
    }

    public List<RelationRow> GetRelationsByType(string type, int limit = 20000)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = """
            SELECT r.Id, r.FromObjectId, r.ToObjectId, r.Type, r.SourcePath, r.SourceLine, r.Confidence,
                   f.FullName AS FromFullName, f.Kind AS FromKind,
                   t.FullName AS ToFullName,   t.Kind AS ToKind
            FROM Relations r
            JOIN Objects f ON f.Id = r.FromObjectId
            JOIN Objects t ON t.Id = r.ToObjectId
            WHERE r.Type = $type
            ORDER BY f.FullName, t.FullName
            LIMIT $limit;
            """;
        cmd.Parameters.AddWithValue("$type", type);
        cmd.Parameters.AddWithValue("$limit", limit);
        return ReadRelationRows(cmd);
    }

    private List<RelationRow> QueryRelations(string column, long id, string[]? types, int limit)
    {
        var typeFilter = BuildTypeFilter(types, out var typeParams);
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = $"""
            SELECT r.Id, r.FromObjectId, r.ToObjectId, r.Type, r.SourcePath, r.SourceLine, r.Confidence,
                   f.FullName AS FromFullName, f.Kind AS FromKind,
                   t.FullName AS ToFullName,   t.Kind AS ToKind
            FROM Relations r
            JOIN Objects f ON f.Id = r.FromObjectId
            JOIN Objects t ON t.Id = r.ToObjectId
            WHERE r.{column} = $id{typeFilter}
            ORDER BY r.Type, r.SourcePath, r.SourceLine
            LIMIT $limit;
            """;
        cmd.Parameters.AddWithValue("$id",    id);
        cmd.Parameters.AddWithValue("$limit", limit);
        AddTypeParams(cmd, typeParams);
        return ReadRelationRows(cmd);
    }

    private static string BuildTypeFilter(string[]? types, out Dictionary<string, string> paramMap)
    {
        paramMap = new();
        if (types is null || types.Length == 0) return string.Empty;
        var placeholders = new List<string>();
        for (int i = 0; i < types.Length; i++)
        {
            var p = $"$t{i}";
            placeholders.Add(p);
            paramMap[p] = types[i];
        }
        return $" AND r.Type IN ({string.Join(",", placeholders)})";
    }

    private static void AddTypeParams(SqliteCommand cmd, Dictionary<string, string> paramMap)
    {
        foreach (var kv in paramMap)
            cmd.Parameters.AddWithValue(kv.Key, kv.Value);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Files (incremental indexing)
    // ─────────────────────────────────────────────────────────────────────────

    public string? GetFileHash(string relativePath)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT Hash FROM Files WHERE Path = $p;";
        cmd.Parameters.AddWithValue("$p", relativePath);
        return cmd.ExecuteScalar() as string;
    }

    public void UpsertFileEntry(OneCFileEntry entry)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Files (Path, Hash, LastIndexedAt)
            VALUES ($path, $hash, $ts)
            ON CONFLICT(Path) DO UPDATE SET
                Hash          = excluded.Hash,
                LastIndexedAt = excluded.LastIndexedAt;
            """;
        cmd.Parameters.AddWithValue("$path", entry.Path);
        cmd.Parameters.AddWithValue("$hash", entry.Hash);
        cmd.Parameters.AddWithValue("$ts",   entry.LastIndexedAt);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Delete all Relations whose SourcePath starts with the given relative path prefix.
    /// Used to clear stale data before re-indexing a modified file.
    /// </summary>
    public void DeleteRelationsBySourcePath(string sourcePath)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Relations WHERE SourcePath = $p;";
        cmd.Parameters.AddWithValue("$p", sourcePath);
        cmd.ExecuteNonQuery();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static List<OneCObject> ReadObjects(SqliteCommand cmd)
    {
        var list = new List<OneCObject>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new OneCObject
            {
                Id       = reader.GetInt64(0),
                Kind     = reader.GetString(1),
                Name     = reader.GetString(2),
                FullName = reader.GetString(3),
                Path     = reader.IsDBNull(4) ? null : reader.GetString(4),
                Summary  = reader.IsDBNull(5) ? null : reader.GetString(5),
            });
        }
        return list;
    }

    private static List<RelationRow> ReadRelationRows(SqliteCommand cmd)
    {
        var list = new List<RelationRow>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new RelationRow
            {
                Id             = reader.GetInt64(0),
                FromObjectId   = reader.GetInt64(1),
                ToObjectId     = reader.GetInt64(2),
                Type           = reader.GetString(3),
                SourcePath     = reader.IsDBNull(4) ? null : reader.GetString(4),
                SourceLine     = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                Confidence     = reader.GetString(6),
                FromFullName   = reader.GetString(7),
                FromKind       = reader.GetString(8),
                ToFullName     = reader.GetString(9),
                ToKind         = reader.GetString(10),
            });
        }
        return list;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _conn.Dispose();
    }
}

/// <summary>
/// Denormalised relation row returned from JOIN queries — includes object names
/// so callers don't need a second round-trip.
/// </summary>
public class RelationRow
{
    public long    Id           { get; set; }
    public long    FromObjectId { get; set; }
    public long    ToObjectId   { get; set; }
    public string  Type         { get; set; } = string.Empty;
    public string? SourcePath   { get; set; }
    public int?    SourceLine   { get; set; }
    public string  Confidence   { get; set; } = string.Empty;
    public string  FromFullName { get; set; } = string.Empty;
    public string  FromKind     { get; set; } = string.Empty;
    public string  ToFullName   { get; set; } = string.Empty;
    public string  ToKind       { get; set; } = string.Empty;
}

