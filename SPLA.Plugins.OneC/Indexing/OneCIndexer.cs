using System.Diagnostics;
using System.Security.Cryptography;
using SPLA.Plugins.OneC.Models;
using SPLA.Plugins.OneC.Storage;

namespace SPLA.Plugins.OneC.Indexing;

/// <summary>
/// Orchestrates a full or incremental indexing run over a 1C configuration dump.
///
/// Run order:
///   1. Scan directory → classify all relevant files
///   2. First pass: parse all metadata XML files → populate Objects table
///   3. Build runtime lookup sets (known kinds, known module names)
///   4. Second pass: analyse BSL files → resolve CodeRelations → insert into Relations
///   5. Update Files table with new hashes
/// </summary>
public class OneCIndexer
{
    private readonly OneCIndexDatabase    _db;
    private readonly OneCFileScanner      _scanner     = new();
    private readonly OneCMetadataParser   _metaParser  = new();
    private readonly OneCCodeAnalyzer     _codeAnalyzer = new();
    private readonly OneCQueryAnalyzer    _queryAnalyzer = new();

    public OneCIndexer(OneCIndexDatabase db)
    {
        _db = db;
        _db.EnsureCreated();
    }

    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Index or re-index a configuration dump directory.
    /// Files whose SHA-256 hash has not changed since the last run are skipped.
    /// </summary>
    public IndexingReport Index(string rootPath, IProgress<string>? progress = null)
    {
        var sw     = Stopwatch.StartNew();
        var report = new IndexingReport();

        // ── 1. Scan ───────────────────────────────────────────────────────────
        List<ScannedFile> files;
        try
        {
            files = _scanner.Scan(rootPath);
        }
        catch (Exception ex)
        {
            report.Errors.Add($"Scan failed: {ex.Message}");
            report.FilesWithErrors++;
            return report;
        }

        progress?.Report($"Found {files.Count} files. Starting first pass (metadata)…");

        // ── 2. First pass — metadata XML → Objects + owns ────────────────────
        var metaFiles = files.Where(f => f.ContentType == FileContentType.Metadata).ToList();
        foreach (var f in metaFiles)
        {
            try
            {
                var hash    = ComputeHash(f.AbsolutePath);
                var stored  = _db.GetFileHash(f.RelativePath);

                if (stored == hash)
                {
                    report.FilesSkipped++;
                    continue;
                }

                var parsed = _metaParser.Parse(f.AbsolutePath, f.ObjectKind, f.ObjectName);
                if (parsed is null)
                {
                    report.FilesWithErrors++;
                    report.Errors.Add($"Failed to parse metadata: {f.RelativePath}");
                    continue;
                }

                // Upsert the top-level object
                var parentId = _db.UpsertObject(new OneCObject
                {
                    Kind     = parsed.Kind,
                    Name     = parsed.Name,
                    FullName = parsed.FullName,
                    Path     = System.IO.Path.GetDirectoryName(f.RelativePath),
                    Summary  = parsed.Summary,
                });
                if (stored is null) report.ObjectsAdded++;
                else                report.ObjectsUpdated++;

                // Upsert child objects (owns relations)
                foreach (var child in parsed.OwnedChildren)
                {
                    var childId = _db.UpsertObject(new OneCObject
                    {
                        Kind     = child.ChildKind,
                        Name     = child.ChildName,
                        FullName = child.ChildFullName,
                        Path     = null,
                        Summary  = null,
                    });
                    report.ObjectsAdded++;

                    _db.UpsertRelation(new OneCRelation
                    {
                        FromObjectId = parentId,
                        ToObjectId   = childId,
                        Type         = RelationType.Owns,
                        SourcePath   = f.RelativePath,
                        Confidence   = RelationConfidence.Exact,
                    });
                    report.RelationsAdded++;
                }

                _db.UpsertFileEntry(new OneCFileEntry
                {
                    Path          = f.RelativePath,
                    Hash          = hash,
                    LastIndexedAt = DateTime.UtcNow.ToString("O"),
                });
            }
            catch (Exception ex)
            {
                report.FilesWithErrors++;
                report.Errors.Add($"Error processing {f.RelativePath}: {ex.Message}");
            }
        }

        progress?.Report("First pass complete. Building lookup tables…");

        // ── 3. Build lookup tables ────────────────────────────────────────────
        // These are used by the code analyzer to resolve names to known objects.
        var commonModules = new HashSet<string>(
            _db.GetObjectsByKind("CommonModule").Select(o => o.Name),
            StringComparer.OrdinalIgnoreCase);

        progress?.Report($"Known common modules: {commonModules.Count}. Starting second pass (BSL)…");

        // ── 4. Second pass — BSL files → Relations ────────────────────────────
        var bslFiles = files.Where(f => f.ContentType == FileContentType.Module).ToList();
        foreach (var f in bslFiles)
        {
            try
            {
                var hash   = ComputeHash(f.AbsolutePath);
                var stored = _db.GetFileHash(f.RelativePath);

                if (stored == hash)
                {
                    report.FilesSkipped++;
                    continue;
                }

                // Clear stale relations from this file before re-inserting
                if (stored is not null)
                    _db.DeleteRelationsBySourcePath(f.RelativePath);

                // Resolve the owning object
                var ownerId = _db.GetObjectId(f.ObjectFullName);
                if (ownerId is null)
                {
                    // Object may not have a metadata file (e.g. pure BSL module);
                    // create a stub entry so relations are not orphaned.
                    ownerId = _db.UpsertObject(new OneCObject
                    {
                        Kind     = f.ObjectKind,
                        Name     = f.ObjectName,
                        FullName = f.ObjectFullName,
                        Path     = System.IO.Path.GetDirectoryName(f.RelativePath),
                    });
                }

                // Code analysis (calls / reads / writes / uses)
                var codeRels = _codeAnalyzer.Analyze(f.AbsolutePath, f.RelativePath, commonModules);

                // Query analysis (queries)
                var queryRels = _queryAnalyzer.Analyze(f.AbsolutePath, f.RelativePath);

                var allRels = codeRels.Concat(queryRels);
                foreach (var cr in allRels)
                {
                    var targetFullName = $"{cr.TargetKind}.{cr.TargetName}";
                    var targetId       = _db.GetObjectId(targetFullName);
                    if (targetId is null) continue; // unknown target — skip

                    _db.UpsertRelation(new OneCRelation
                    {
                        FromObjectId = ownerId.Value,
                        ToObjectId   = targetId.Value,
                        Type         = cr.RelationType,
                        SourcePath   = cr.SourcePath,
                        SourceLine   = cr.SourceLine,
                        Confidence   = cr.Confidence,
                    });
                    report.RelationsAdded++;
                }

                _db.UpsertFileEntry(new OneCFileEntry
                {
                    Path          = f.RelativePath,
                    Hash          = hash,
                    LastIndexedAt = DateTime.UtcNow.ToString("O"),
                });
            }
            catch (Exception ex)
            {
                report.FilesWithErrors++;
                report.Errors.Add($"Error processing {f.RelativePath}: {ex.Message}");
            }
        }

        sw.Stop();
        report.Elapsed = sw.Elapsed;
        progress?.Report($"Indexing complete: {report}");
        return report;
    }

    // ─────────────────────────────────────────────────────────────────────────

    private static string ComputeHash(string path)
    {
        using var sha  = SHA256.Create();
        using var fs   = File.OpenRead(path);
        var bytes = sha.ComputeHash(fs);
        return Convert.ToHexString(bytes);
    }
}

