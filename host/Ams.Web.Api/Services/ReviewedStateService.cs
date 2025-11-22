using System.Text.Json;
using System.Text.Json.Serialization;
using Ams.Web.Api.Json;
using Ams.Web.Shared.Validation;

namespace Ams.Web.Api.Services;

public sealed class ReviewedStateService
{
    private readonly object _sync = new();
    private readonly WorkspaceState _state;

    public ReviewedStateService(WorkspaceState state)
    {
        _state = state;
    }

    public Dictionary<string, ReviewedStatusDto> Get(string bookId)
    {
        lock (_sync)
        {
            var all = Load();
            return all.TryGetValue(bookId, out var book)
                ? new Dictionary<string, ReviewedStatusDto>(book, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, ReviewedStatusDto>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public Dictionary<string, ReviewedStatusDto> Set(string bookId, string chapterId, bool reviewed)
    {
        lock (_sync)
        {
            var all = Load();
            if (!all.TryGetValue(bookId, out var book))
            {
                book = new Dictionary<string, ReviewedStatusDto>(StringComparer.OrdinalIgnoreCase);
                all[bookId] = book;
            }

            book[chapterId] = new ReviewedStatusDto(reviewed, DateTime.UtcNow.ToString("o"));
            Save(all);
            return new Dictionary<string, ReviewedStatusDto>(book, StringComparer.OrdinalIgnoreCase);
        }
    }

    public void Reset(string bookId)
    {
        lock (_sync)
        {
            var all = Load();
            if (all.Remove(bookId))
            {
                Save(all);
            }
        }
    }

    private Dictionary<string, Dictionary<string, ReviewedStatusDto>> Load()
    {
        var file = GetFile();
        if (!file.Exists) return new(StringComparer.OrdinalIgnoreCase);

        try
        {
            var json = File.ReadAllText(file.FullName);
            var data = JsonSerializer.Deserialize(json, ApiJsonSerializerContext.Default.ReviewedStore);
            return data ?? new(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new(StringComparer.OrdinalIgnoreCase);
        }
    }

    private void Save(Dictionary<string, Dictionary<string, ReviewedStatusDto>> payload)
    {
        try
        {
            var file = GetFile();
            file.Directory?.Create();
            var json = JsonSerializer.Serialize(payload, ApiJsonSerializerContext.Default.ReviewedStore);
            File.WriteAllText(file.FullName, json);
        }
        catch
        {
            // best-effort
        }
    }

    private FileInfo GetFile()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrWhiteSpace(root))
        {
            root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
        }

        return new FileInfo(Path.Combine(root, "AMS", "validation-viewer", "reviewed.json"));
    }
}
