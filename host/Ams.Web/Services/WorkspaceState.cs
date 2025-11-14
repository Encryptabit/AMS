using System.Text.Json;
using Ams.Web.Configuration;
using Ams.Web.Requests;
using Ams.Web.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ams.Web.Services;

public sealed class WorkspaceState
{
    private readonly IHostEnvironment _environment;
    private readonly ILogger<WorkspaceState> _logger;
    private readonly string _stateFilePath = AppDataPaths.Resolve("workspace-state.json");
    private readonly SemaphoreSlim _gate = new(1, 1);

    private string _workspaceRoot;
    private string? _bookIndexPath;
    private string? _templatePath;

    public event EventHandler<WorkspaceSnapshot>? Changed;

    public WorkspaceState(IOptions<AmsOptions> options, IHostEnvironment environment, ILogger<WorkspaceState> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var defaults = options.Value;
        _workspaceRoot = ResolveDirectoryOrDefault(defaults.Workspace.RootPath, "analysis");
        _bookIndexPath = defaults.Workspace.BookIndexPath is null ? null : ResolveFileOrDefault(defaults.Workspace.BookIndexPath, "analysis/book-index.json");
        _templatePath = defaults.Crx.TemplatePath is null ? null : ResolveFileOrDefault(defaults.Crx.TemplatePath, "analysis/CRX_Template.xlsx");

        LoadPersistedState();
    }

    public WorkspaceSnapshot Snapshot => new(
        WorkspaceRoot: _workspaceRoot,
        BookIndexPath: BookIndexPath,
        CrxTemplatePath: TemplatePath);

    public string WorkspaceRoot => _workspaceRoot;

    public string BookIndexPath => _bookIndexPath ?? Path.Combine(_workspaceRoot, "book-index.json");

    public string TemplatePath => _templatePath ?? Path.Combine(_workspaceRoot, "CRX_Template.xlsx");

    public async Task UpdateAsync(WorkspaceUpdateRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var changed = false;
            if (!string.IsNullOrWhiteSpace(request.WorkspaceRoot))
            {
                var resolved = PathUtilities.ResolveUserPath(request.WorkspaceRoot!, _environment);
                if (!Directory.Exists(resolved))
                {
                    throw new DirectoryNotFoundException($"Workspace directory not found: {resolved}");
                }

                if (!string.Equals(resolved, _workspaceRoot, StringComparison.OrdinalIgnoreCase))
                {
                    _workspaceRoot = resolved;
                    changed = true;
                }
            }

            if (!string.IsNullOrWhiteSpace(request.BookIndexPath))
            {
                var resolved = PathUtilities.ResolveUserPath(request.BookIndexPath!, _environment);
                if (!File.Exists(resolved))
                {
                    throw new FileNotFoundException($"BookIndex file not found: {resolved}", resolved);
                }

                if (!string.Equals(resolved, _bookIndexPath, StringComparison.OrdinalIgnoreCase))
                {
                    _bookIndexPath = resolved;
                    changed = true;
                }
            }

            if (!string.IsNullOrWhiteSpace(request.CrxTemplatePath))
            {
                var resolved = PathUtilities.ResolveUserPath(request.CrxTemplatePath!, _environment);
                if (!File.Exists(resolved))
                {
                    throw new FileNotFoundException($"CRX template not found: {resolved}", resolved);
                }

                if (!string.Equals(resolved, _templatePath, StringComparison.OrdinalIgnoreCase))
                {
                    _templatePath = resolved;
                    changed = true;
                }
            }

            if (changed)
            {
                Persist();
                Changed?.Invoke(this, Snapshot);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private string ResolveDirectoryOrDefault(string? candidate, string fallback)
    {
        var source = string.IsNullOrWhiteSpace(candidate) ? fallback : candidate!;
        var resolved = PathUtilities.ResolveUserPath(source, _environment);
        Directory.CreateDirectory(resolved);
        return resolved;
    }

    private string ResolveFileOrDefault(string? candidate, string fallback)
    {
        var source = string.IsNullOrWhiteSpace(candidate) ? fallback : candidate!;
        return PathUtilities.ResolveUserPath(source, _environment);
    }

    private void LoadPersistedState()
    {
        try
        {
            if (!File.Exists(_stateFilePath))
            {
                return;
            }

            var json = File.ReadAllText(_stateFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            var payload = JsonSerializer.Deserialize<WorkspaceSnapshot>(json);
            if (payload is null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(payload.WorkspaceRoot) && Directory.Exists(payload.WorkspaceRoot))
            {
                _workspaceRoot = payload.WorkspaceRoot;
            }

            if (!string.IsNullOrWhiteSpace(payload.BookIndexPath) && File.Exists(payload.BookIndexPath))
            {
                _bookIndexPath = payload.BookIndexPath;
            }

            if (!string.IsNullOrWhiteSpace(payload.CrxTemplatePath) && File.Exists(payload.CrxTemplatePath))
            {
                _templatePath = payload.CrxTemplatePath;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load workspace state from {Path}", _stateFilePath);
        }
    }

    private void Persist()
    {
        try
        {
            var directory = Path.GetDirectoryName(_stateFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var snapshot = Snapshot;
            var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_stateFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist workspace state to {Path}", _stateFilePath);
        }
    }
}

public sealed record WorkspaceSnapshot(
    string WorkspaceRoot,
    string BookIndexPath,
    string CrxTemplatePath);
