namespace SPLA.Plugins.Sql.Safety;

public enum SqlRiskLevel
{
    Safe,       // SELECT — read-only, no confirmation needed
    Moderate,   // DML with WHERE — run query plan first, confirm if rows > threshold
    High,       // DML without WHERE / TRUNCATE — always confirm
    Forbidden   // DDL (DROP, ALTER, CREATE) — blocked in non-agent modes
}

public record SqlRiskAssessment(
    SqlRiskLevel Level,
    string Reason,
    bool RequiresConfirmation,
    bool RequiresQueryPlan);

public static class SqlSafetyAnalyzer
{
    private static readonly int LargeRowThreshold = 1000;

    /// <summary>
    /// Statically analyses a SQL statement and returns its risk level.
    /// This is a fast pre-flight check before execution.
    /// </summary>
    public static SqlRiskAssessment Analyze(string sql)
    {
        var normalized = NormalizeSql(sql);
        var firstToken = GetFirstToken(normalized);

        return firstToken switch
        {
            "select" or "with" or "explain" =>
                new(SqlRiskLevel.Safe, "Read-only query", false, false),

            "insert" or "update" or "delete" =>
                AnalyzeDml(normalized, firstToken),

            "truncate" =>
                new(SqlRiskLevel.High,
                    "TRUNCATE removes all rows without WHERE — cannot be rolled back.",
                    RequiresConfirmation: true,
                    RequiresQueryPlan: false),

            "drop" or "alter" or "create" or "rename" =>
                new(SqlRiskLevel.Forbidden,
                    $"DDL statement ({firstToken.ToUpperInvariant()}) is not allowed via sql_execute. Use a DB admin tool.",
                    RequiresConfirmation: false,
                    RequiresQueryPlan: false),

            _ =>
                new(SqlRiskLevel.Moderate,
                    $"Unknown statement type '{firstToken}' — treating as potentially destructive.",
                    RequiresConfirmation: true,
                    RequiresQueryPlan: false)
        };
    }

    /// <summary>
    /// Checks estimated rows from a query plan output and returns a warning if dangerous.
    /// Returns null when the row count is within safe limits.
    /// </summary>
    public static string? CheckEstimatedRows(int estimatedRows)
    {
        if (estimatedRows > LargeRowThreshold)
            return $"Query plan estimates {estimatedRows} affected rows (threshold: {LargeRowThreshold}). Confirm to proceed.";
        return null;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static SqlRiskAssessment AnalyzeDml(string normalized, string verb)
    {
        var hasWhere = HasWhereClause(normalized);

        if (!hasWhere)
            return new(
                SqlRiskLevel.High,
                $"{verb.ToUpperInvariant()} without WHERE affects all rows in the table.",
                RequiresConfirmation: true,
                RequiresQueryPlan: false);

        return new(
            SqlRiskLevel.Moderate,
            $"{verb.ToUpperInvariant()} with WHERE — will estimate affected rows via query plan.",
            RequiresConfirmation: true,
            RequiresQueryPlan: true);
    }

    private static bool HasWhereClause(string normalized)
    {
        // Looks for standalone WHERE keyword (not inside subqueries — good enough for a safety hint)
        // TODO: replace with a proper SQL parser for edge cases (CTE, subquery aliases)
        return System.Text.RegularExpressions.Regex.IsMatch(
            normalized, @"\bwhere\b",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private static string NormalizeSql(string sql)
        => System.Text.RegularExpressions.Regex.Replace(sql.Trim(), @"\s+", " ").ToLowerInvariant();

    private static string GetFirstToken(string normalized)
    {
        var match = System.Text.RegularExpressions.Regex.Match(normalized, @"^\s*(\w+)");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }
}
