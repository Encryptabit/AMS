using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Ams.Core.Common;

/// <summary>
/// Central logging façade for AMS components.
/// Provides a uniform logging pipeline based on Microsoft.Extensions.Logging and Serilog.
/// </summary>
public static class Log
{
    private const string DefaultCategory = "AMS";
    private const string LevelEnvVar = "AMS_LOG_LEVEL";

    private static readonly object SyncRoot = new();
    private static ILoggerFactory loggerFactory = CreateDefaultFactory();
    private static ILogger logger = loggerFactory.CreateLogger(DefaultCategory);

    public static string? LogDirectory { get; private set; }
    public static string? LogFilePath { get; private set; }

    /// <summary>
    /// Creates a preconfigured logger factory that writes to console and rolling text files.
    /// Consumers can share this to keep log formatting uniform.
    /// </summary>
    public static ILoggerFactory CreateDefaultFactory(
        string? baseDirectory = null,
        string logFileName = "ams-log.txt",
        long fileSizeLimitBytes = 10 * 1024 * 1024,
        int retainedFileCountLimit = 5,
        bool includeConsole = true,
        LogEventLevel? minimumLevel = null)
    {
        var baseDir = baseDirectory;
        if (string.IsNullOrWhiteSpace(baseDir))
        {
            baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(baseDir))
            {
                baseDir = AppContext.BaseDirectory;
            }
        }

        LogDirectory = Path.Combine(baseDir!, "AMS", "logs");
        Directory.CreateDirectory(LogDirectory);
        LogFilePath = Path.Combine(LogDirectory, logFileName);

        var resolvedLevel = minimumLevel ?? ResolveMinimumLevelFromEnvironment();

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(resolvedLevel)
            .Enrich.FromLogContext();

        if (includeConsole)
        {
            loggerConfiguration = loggerConfiguration.WriteTo.Console(
                outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
        }

        loggerConfiguration = loggerConfiguration.WriteTo.File(
            LogFilePath,
            rollingInterval: RollingInterval.Infinite,
            fileSizeLimitBytes: fileSizeLimitBytes,
            rollOnFileSizeLimit: true,
            retainedFileCountLimit: retainedFileCountLimit,
            shared: true,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

        var serilogLogger = loggerConfiguration.CreateLogger();

        return LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(serilogLogger, dispose: true);
        });
    }

    /// <summary>
    /// Configures the shared logger using the default factory settings and returns the factory.
    /// </summary>
    public static ILoggerFactory ConfigureDefaults(
        string? baseDirectory = null,
        string logFileName = "ams-log.txt",
        long fileSizeLimitBytes = 10 * 1024 * 1024,
        int retainedFileCountLimit = 5,
        bool includeConsole = true,
        LogEventLevel? minimumLevel = null)
    {
        var factory = CreateDefaultFactory(baseDirectory, logFileName, fileSizeLimitBytes, retainedFileCountLimit,
            includeConsole, minimumLevel);
        Configure(factory);
        return factory;
    }

    public static bool IsDebugLoggingEnabled()
    {
        var resolvedLevel = ResolveMinimumLevelFromEnvironment();
        return resolvedLevel <= LogEventLevel.Debug;
    }

    public static void Configure(ILoggerFactory factory, string? category = null)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        lock (SyncRoot)
        {
            loggerFactory = factory;
            logger = loggerFactory.CreateLogger(string.IsNullOrWhiteSpace(category) ? DefaultCategory : category);
        }
    }

    public static ILogger For<T>() => loggerFactory.CreateLogger<T>();

    public static ILogger For(string categoryName) => loggerFactory.CreateLogger(categoryName);

    public static IDisposable BeginScope<TState>(TState state) where TState : notnull => logger.BeginScope(state)!;

    public static void Trace(string message, params object?[] args) => logger.LogTrace(message, args);

    public static void Debug(string message, params object?[] args) => logger.LogDebug(message, args);

    public static void Info(string message, params object?[] args) => logger.LogInformation(message, args);

    public static void Warn(string message, params object?[] args) => logger.LogWarning(message, args);

    public static void Error(string message, params object?[] args) => logger.LogError(message, args);

    public static void Error(Exception exception, string message, params object?[] args) =>
        logger.LogError(exception, message, args);

    public static void Critical(Exception exception, string message, params object?[] args) =>
        logger.LogCritical(exception, message, args);

    private static LogEventLevel ResolveMinimumLevelFromEnvironment()
    {
        var raw = Environment.GetEnvironmentVariable(LevelEnvVar);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return LogEventLevel.Information;
        }

        return raw.Trim().ToUpperInvariant() switch
        {
            "TRACE" or "VERBOSE" => LogEventLevel.Verbose,
            "DEBUG" => LogEventLevel.Debug,
            "INFO" or "INFORMATION" => LogEventLevel.Information,
            "WARN" or "WARNING" => LogEventLevel.Warning,
            "ERROR" => LogEventLevel.Error,
            "FATAL" or "CRITICAL" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
}