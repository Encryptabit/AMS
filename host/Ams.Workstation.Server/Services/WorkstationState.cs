using System.Text.Json;
using Ams.Core.Runtime.Workspace;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Scoped service that maintains workstation session state.
/// Each Blazor circuit gets its own instance.
/// State is persisted to %LOCALAPPDATA%\AMS\workstation-state.json.
/// </summary>
public class WorkstationState : IDisposable
{
    private static readonly string StateFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AMS",
        "workstation-state.json");

    private BlazorWorkspace? _workspace;
    private bool _disposed;
    private string? _currentChapter;

    public WorkstationState()
    {
        LoadPersistedState();
    }

    /// <summary>
    /// Path to the working directory containing pipeline artifacts.
    /// </summary>
    public string? WorkingDirectory { get; private set; }

    /// <summary>
    /// Name of the currently selected chapter for review.
    /// </summary>
    public string? CurrentChapter
    {
        get => _currentChapter;
        set
        {
            _currentChapter = value;
            SavePersistedState();
        }
    }

    /// <summary>
    /// List of available chapter names from the loaded book.
    /// </summary>
    public List<string> AvailableChapters { get; private set; } = new();

    /// <summary>
    /// Indicates whether a book-index.json exists in the working directory.
    /// </summary>
    public bool HasBookIndex => !string.IsNullOrEmpty(WorkingDirectory)
        && File.Exists(Path.Combine(WorkingDirectory, "book-index.json"));

    /// <summary>
    /// The active workspace, or null if not initialized.
    /// </summary>
    public IWorkspace? Workspace => _workspace;

    /// <summary>
    /// Sets the working directory and initializes the workspace.
    /// </summary>
    /// <param name="path">Path to the working directory.</param>
    /// <returns>True if successful, false if the path is invalid.</returns>
    public bool SetWorkingDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;

        var trimmed = path.Trim();
        if (!Directory.Exists(trimmed)) return false;

        // Dispose previous workspace
        _workspace?.Dispose();
        _workspace = null;

        WorkingDirectory = trimmed;
        CurrentChapter = null;
        AvailableChapters.Clear();

        // Load chapters if book-index.json exists
        if (HasBookIndex)
        {
            LoadChaptersFromIndex();
            _workspace = new BlazorWorkspace(trimmed);
        }

        SavePersistedState();
        return true;
    }

    private void LoadChaptersFromIndex()
    {
        var indexPath = Path.Combine(WorkingDirectory!, "book-index.json");
        try
        {
            using var stream = File.OpenRead(indexPath);
            using var doc = JsonDocument.Parse(stream);

            // Book-index uses "sections" array with title, level, kind, etc.
            if (doc.RootElement.TryGetProperty("sections", out var sections))
            {
                foreach (var section in sections.EnumerateArray())
                {
                    if (section.TryGetProperty("title", out var titleProp))
                    {
                        var title = titleProp.GetString();
                        if (!string.IsNullOrEmpty(title))
                        {
                            AvailableChapters.Add(title);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw - chapters will be empty
            Console.WriteLine($"Failed to load book-index.json: {ex.Message}");
        }
    }

    /// <summary>
    /// Clears all state, resetting to initial values.
    /// </summary>
    public void Clear()
    {
        _workspace?.Dispose();
        _workspace = null;
        WorkingDirectory = null;
        _currentChapter = null;
        AvailableChapters.Clear();
        SavePersistedState();
    }

    private void LoadPersistedState()
    {
        try
        {
            if (!File.Exists(StateFilePath)) return;

            var json = File.ReadAllText(StateFilePath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("workingDirectory", out var wdProp))
            {
                var wd = wdProp.GetString();
                if (!string.IsNullOrEmpty(wd) && Directory.Exists(wd))
                {
                    // Initialize workspace from persisted path
                    SetWorkingDirectory(wd);

                    // Restore current chapter if still valid
                    if (root.TryGetProperty("currentChapter", out var chProp))
                    {
                        var ch = chProp.GetString();
                        if (!string.IsNullOrEmpty(ch) && AvailableChapters.Contains(ch))
                        {
                            _currentChapter = ch;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load persisted state: {ex.Message}");
        }
    }

    private void SavePersistedState()
    {
        try
        {
            var dir = Path.GetDirectoryName(StateFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var state = new
            {
                workingDirectory = WorkingDirectory,
                currentChapter = _currentChapter
            };

            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(StateFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save persisted state: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _workspace?.Dispose();
    }
}
