namespace SPLA.Plugins.OneC.Storage;

/// <summary>
/// DDL statements and index definitions for the onec.sqlite database.
/// </summary>
internal static class OneCIndexSchema
{
    public const string CreateObjects = """
        CREATE TABLE IF NOT EXISTS Objects (
            Id       INTEGER PRIMARY KEY AUTOINCREMENT,
            Kind     TEXT NOT NULL,
            Name     TEXT NOT NULL,
            FullName TEXT NOT NULL,
            Path     TEXT,
            Summary  TEXT,
            UNIQUE (FullName)
        );
        """;

    public const string CreateRelations = """
        CREATE TABLE IF NOT EXISTS Relations (
            Id           INTEGER PRIMARY KEY AUTOINCREMENT,
            FromObjectId INTEGER NOT NULL,
            ToObjectId   INTEGER NOT NULL,
            Type         TEXT    NOT NULL,
            SourcePath   TEXT,
            SourceLine   INTEGER,
            Confidence   TEXT    NOT NULL DEFAULT 'heuristic',
            FOREIGN KEY (FromObjectId) REFERENCES Objects(Id),
            FOREIGN KEY (ToObjectId)   REFERENCES Objects(Id),
            UNIQUE (FromObjectId, ToObjectId, Type, SourcePath, SourceLine)
        );
        """;

    public const string CreateFiles = """
        CREATE TABLE IF NOT EXISTS Files (
            Id            INTEGER PRIMARY KEY AUTOINCREMENT,
            Path          TEXT NOT NULL UNIQUE,
            Hash          TEXT NOT NULL,
            LastIndexedAt TEXT NOT NULL
        );
        """;

    // ── Indexes ──────────────────────────────────────────────────────────────

    public const string IdxObjectsFullName =
        "CREATE UNIQUE INDEX IF NOT EXISTS idx_objects_fullname ON Objects(FullName);";

    public const string IdxObjectsKind =
        "CREATE INDEX IF NOT EXISTS idx_objects_kind ON Objects(Kind);";

    public const string IdxRelationsFrom =
        "CREATE INDEX IF NOT EXISTS idx_relations_from ON Relations(FromObjectId, Type);";

    public const string IdxRelationsTo =
        "CREATE INDEX IF NOT EXISTS idx_relations_to ON Relations(ToObjectId, Type);";

    public const string IdxFilesPath =
        "CREATE UNIQUE INDEX IF NOT EXISTS idx_files_path ON Files(Path);";

    /// <summary>All DDL statements in creation order.</summary>
    public static readonly string[] All =
    [
        CreateObjects,
        CreateRelations,
        CreateFiles,
        IdxObjectsFullName,
        IdxObjectsKind,
        IdxRelationsFrom,
        IdxRelationsTo,
        IdxFilesPath,
    ];
}

