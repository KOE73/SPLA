using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace SPLA.Observability;

public sealed record SplaTelemetryContext(
    string? ConversationId = null,
    string? RequestId = null,
    string? MessageId = null,
    string? ToolCallId = null,
    string? ProjectId = null,
    string? WorkspacePath = null);

public static class SplaTelemetry
{
    public const string ServiceName = "SPLA";

    public static readonly ActivitySource ActivitySource = new(ServiceName);
    public static readonly Meter Meter = new(ServiceName);
    public static readonly Counter<long> ToolCalls = Meter.CreateCounter<long>("spla.tool.calls");
    public static readonly Counter<long> ToolErrors = Meter.CreateCounter<long>("spla.tool.errors");
    public static readonly Histogram<double> ToolDurationMs = Meter.CreateHistogram<double>("spla.tool.duration.ms");

    private static readonly AsyncLocal<SplaTelemetryContext?> CurrentContextSlot = new();
    private static readonly SplaFileLoggerProvider FileLoggerProvider = new();

    public static SplaTelemetryContext? CurrentContext
    {
        get => CurrentContextSlot.Value;
        set => CurrentContextSlot.Value = value;
    }

    public static ILoggerProvider CreateFileLoggerProvider() => FileLoggerProvider;

    public static void ConfigureGlobalLogs(string applicationName = "SPLA")
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var logsDirectory = Path.Combine(localAppData, applicationName, "logs");
        FileLoggerProvider.SetGlobalLogDirectory(logsDirectory);
    }

    public static void ConfigureProjectLogs(string? workspacePath)
    {
        var logsDirectory = string.IsNullOrWhiteSpace(workspacePath)
            ? null
            : Path.Combine(workspacePath, ".spla", "logs");

        FileLoggerProvider.SetProjectLogDirectory(logsDirectory);
    }

    public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        var activity = ActivitySource.StartActivity(name, kind);
        Enrich(activity);
        return activity;
    }

    public static IDisposable PushContext(SplaTelemetryContext context)
    {
        var previous = CurrentContext;
        CurrentContext = context;
        return new PopContext(previous);
    }

    public static void Enrich(Activity? activity)
    {
        if (activity is null || CurrentContext is not { } context) return;

        SetTag(activity, "spla.conversation_id", context.ConversationId);
        SetTag(activity, "spla.request_id", context.RequestId);
        SetTag(activity, "spla.message_id", context.MessageId);
        SetTag(activity, "spla.tool_call_id", context.ToolCallId);
        SetTag(activity, "spla.project_id", context.ProjectId);
        SetTag(activity, "spla.workspace_path", context.WorkspacePath);
    }

    private static void SetTag(Activity activity, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            activity.SetTag(name, value);
        }
    }

    private sealed class PopContext(SplaTelemetryContext? previous) : IDisposable
    {
        public void Dispose() => CurrentContext = previous;
    }
}

public sealed class SplaFileLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly ConcurrentDictionary<string, SplaFileLogger> _loggers = new(StringComparer.Ordinal);
    private readonly object _sync = new();
    private IExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();
    private string? _globalLogDirectory;
    private string? _projectLogDirectory;

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, category => new(category, this));

    public void Dispose() => _loggers.Clear();

    public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopeProvider = scopeProvider;

    public void SetGlobalLogDirectory(string? directory)
    {
        lock (_sync)
        {
            _globalLogDirectory = directory;
            EnsureDirectory(directory);
        }
    }

    public void SetProjectLogDirectory(string? directory)
    {
        lock (_sync)
        {
            _projectLogDirectory = directory;
            EnsureDirectory(directory);
        }
    }

    internal void Write<TState>(
        string category,
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (logLevel == LogLevel.None) return;

        var message = formatter(state, exception);
        if (string.IsNullOrWhiteSpace(message) && exception is null) return;

        var line = FormatLine(category, logLevel, eventId, message, exception);

        lock (_sync)
        {
            foreach (var directory in GetActiveDirectories())
            {
                EnsureDirectory(directory);
                File.AppendAllText(GetLogPath(directory), line);
            }
        }
    }

    private IEnumerable<string> GetActiveDirectories()
    {
        if (!string.IsNullOrWhiteSpace(_globalLogDirectory))
        {
            yield return _globalLogDirectory;
        }

        if (!string.IsNullOrWhiteSpace(_projectLogDirectory)
            && !string.Equals(_projectLogDirectory, _globalLogDirectory, StringComparison.OrdinalIgnoreCase))
        {
            yield return _projectLogDirectory;
        }
    }

    private string FormatLine(string category, LogLevel logLevel, EventId eventId, string message, Exception? exception)
    {
        var timestamp = DateTimeOffset.Now.ToString("O");
        var activity = Activity.Current;
        var context = SplaTelemetry.CurrentContext;
        var traceId = activity?.TraceId.ToString() ?? "";
        var spanId = activity?.SpanId.ToString() ?? "";
        var eventName = string.IsNullOrWhiteSpace(eventId.Name) ? eventId.Id.ToString() : eventId.Name;
        var contextText = FormatContext(context);
        var exceptionText = exception is null ? "" : Environment.NewLine + exception;

        return $"{timestamp} [{logLevel}] {category} ({eventName}) trace={traceId} span={spanId}{contextText} {message}{exceptionText}{Environment.NewLine}";
    }

    private static string FormatContext(SplaTelemetryContext? context)
    {
        if (context is null) return "";

        return string.Concat(
            FormatValue(" conversation", context.ConversationId),
            FormatValue(" request", context.RequestId),
            FormatValue(" message", context.MessageId),
            FormatValue(" toolCall", context.ToolCallId),
            FormatValue(" project", context.ProjectId),
            FormatValue(" workspace", context.WorkspacePath));
    }

    private static string FormatValue(string key, string? value) =>
        string.IsNullOrWhiteSpace(value) ? "" : $"{key}={value}";

    private static string GetLogPath(string directory)
    {
        var date = DateTimeOffset.Now.ToString("yyyyMMdd");
        var basePath = Path.Combine(directory, $"spla-{date}.log");
        const long maxBytes = 10L * 1024L * 1024L;

        if (!File.Exists(basePath) || new FileInfo(basePath).Length < maxBytes)
        {
            return basePath;
        }

        for (var index = 1; index < 1000; index++)
        {
            var path = Path.Combine(directory, $"spla-{date}.{index:000}.log");
            if (!File.Exists(path) || new FileInfo(path).Length < maxBytes)
            {
                return path;
            }
        }

        return basePath;
    }

    private static void EnsureDirectory(string? directory)
    {
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}

internal sealed class SplaFileLogger(string category, SplaFileLoggerProvider provider) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull =>
        NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter) =>
        provider.Write(category, logLevel, eventId, state, exception, formatter);
}

internal sealed class NullScope : IDisposable
{
    public static readonly NullScope Instance = new();

    public void Dispose()
    {
    }
}
