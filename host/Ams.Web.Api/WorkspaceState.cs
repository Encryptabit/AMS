using System.Text.Json;
using System.Text.Json.Serialization;
using Ams.Core.Runtime.Artifacts;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Workspace;
using Ams.Web.Api.Json;
using Ams.Web.Api.Payloads;

public sealed class WorkspaceState
{
    private readonly object _sync = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    private BookManager? _bookManager;

    public WorkspaceState()
    {
        LoadFromDisk();
    }

    public string? BookRoot { get; private set; }
    public string? BookIndexPath { get; private set; }
    public string? CrxTemplatePath { get; private set; }
    public string CrxDirectoryName { get; private set; } = "CRX";
    public string DefaultErrorType { get; private set; } = "MR";

    public void Update(WorkspaceRequest request)
    {
        lock (_sync)
        {
            if (!string.IsNullOrWhiteSpace(request.WorkspaceRoot))
            {
                BookRoot = request.WorkspaceRoot;
                _bookManager = null; // reset so it will rebuild on next access
            }

            if (!string.IsNullOrWhiteSpace(request.BookIndexPath))
            {
                BookIndexPath = request.BookIndexPath;
            }
            else if (!string.IsNullOrWhiteSpace(BookRoot))
            {
                BookIndexPath ??= Path.Combine(BookRoot, "book-index.json");
            }

            if (!string.IsNullOrWhiteSpace(request.CrxTemplatePath))
            {
                CrxTemplatePath = request.CrxTemplatePath;
            }

            if (!string.IsNullOrWhiteSpace(request.CrxDirectoryName))
            {
                CrxDirectoryName = request.CrxDirectoryName;
            }

            if (!string.IsNullOrWhiteSpace(request.DefaultErrorType))
            {
                DefaultErrorType = request.DefaultErrorType;
            }

            SaveToDisk();
        }
    }

    public BookContext GetBook(string bookId)
    {
        lock (_sync)
        {
            _bookManager ??= BuildBookManager(bookId);
            if (!string.Equals(_bookManager.Current.Descriptor.BookId, bookId, StringComparison.OrdinalIgnoreCase))
            {
                _bookManager.Load(bookId);
            }

            return _bookManager.Current;
        }
    }

    private BookManager BuildBookManager(string bookId)
    {
        if (string.IsNullOrWhiteSpace(BookRoot))
        {
            throw new InvalidOperationException("WorkspaceRoot is not configured.");
        }

        var descriptors = WorkspaceChapterDiscovery.Discover(BookRoot);
        var bookDescriptor = new BookDescriptor(bookId, BookRoot, descriptors);
        return new BookManager(new[] { bookDescriptor }, FileArtifactResolver.Instance);
    }

    public WorkspaceResponse ToResponse() => new(
        WorkspaceRoot: BookRoot,
        BookIndexPath: BookIndexPath,
        CrxTemplatePath: CrxTemplatePath,
        CrxDirectoryName: CrxDirectoryName,
        DefaultErrorType: DefaultErrorType);

    private void LoadFromDisk()
    {
        var file = GetConfigFile();
        if (!file.Exists) return;

        try
        {
            var json = File.ReadAllText(file.FullName);
            var cfg = JsonSerializer.Deserialize(json, ApiJsonSerializerContext.Default.WorkspaceRequest);
            if (cfg is null) return;

            Update(cfg);
        }
        catch
        {
            // ignore malformed config
        }
    }

    private void SaveToDisk()
    {
        try
        {
            var file = GetConfigFile();
            file.Directory?.Create();
            var payload = new WorkspaceRequest(BookRoot, BookIndexPath, CrxTemplatePath, CrxDirectoryName, DefaultErrorType);
            var json = JsonSerializer.Serialize(payload, ApiJsonSerializerContext.Default.WorkspaceRequest);
            File.WriteAllText(file.FullName, json);
        }
        catch
        {
            // best-effort persistence
        }
    }

    private FileInfo GetConfigFile()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrWhiteSpace(root))
        {
            root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
        }

        return new FileInfo(Path.Combine(root, "AMS", "validation-viewer", "workspace.json"));
    }
}
