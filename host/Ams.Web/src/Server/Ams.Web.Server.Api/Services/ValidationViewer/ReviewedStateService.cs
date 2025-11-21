using System.Text.Json;
using System.Text.Json.Serialization;
using Ams.Web.Shared.ValidationViewer;
using Ams.Web.Server.Api.Models.ValidationViewer;
using Microsoft.Extensions.Options;

namespace Ams.Web.Server.Api.Services.ValidationViewer;

internal sealed class ReviewedStateService : IReviewedStateService
{
    private readonly ValidationViewerWorkspaceState _state;
    private readonly IOptions<ValidationViewerOptions> _options;
    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ReviewedStateService(ValidationViewerWorkspaceState state, IOptions<ValidationViewerOptions> options)
    {
        _state = state;
        _options = options;
    }

    public async Task<Dictionary<string, ReviewedStatusDto>> GetAsync(string bookId, CancellationToken ct = default)
    {
        var file = GetReviewedFile();
        if (!file.Exists)
        {
            return new();
        }

        await using var stream = file.OpenRead();
        var all = await JsonSerializer.DeserializeAsync<Dictionary<string, Dictionary<string, ReviewedStatusDto>>>(stream, _json, ct)
                  ?? new();
        return all.TryGetValue(bookId, out var bookDict) ? new(bookDict) : new();
    }

    public async Task<Dictionary<string, ReviewedStatusDto>> SetAsync(string bookId, string chapterId, bool reviewed, CancellationToken ct = default)
    {
        var current = await GetAsync(bookId, ct);
        current[chapterId] = new ReviewedStatusDto(reviewed, DateTime.UtcNow.ToString("o"));
        await SaveAsync(bookId, current, ct);
        return current;
    }

    public async Task ResetReviewsAsync(string bookId, CancellationToken ct = default)
        => await SaveAsync(bookId, new Dictionary<string, ReviewedStatusDto>(), ct);

    public async Task ResetAsync(string bookId, CancellationToken ct = default)
        => await ResetReviewsAsync(bookId, ct);

    private async Task SaveAsync(string bookId, Dictionary<string, ReviewedStatusDto> current, CancellationToken ct)
    {
        var file = GetReviewedFile();
        file.Directory?.Create();

        Dictionary<string, Dictionary<string, ReviewedStatusDto>> payload;
        if (file.Exists)
        {
            await using var existing = file.OpenRead();
            payload = await JsonSerializer.DeserializeAsync<Dictionary<string, Dictionary<string, ReviewedStatusDto>>>(existing, _json, ct)
                       ?? new();
        }
        else
        {
            payload = new();
        }

        payload[bookId] = current;
        await using var stream = file.Open(FileMode.Create, FileAccess.Write, FileShare.Read);
        await JsonSerializer.SerializeAsync(stream, payload, _json, ct);
    }

    private FileInfo GetReviewedFile()
    {
        var opt = _options.Value;
        if (!string.IsNullOrWhiteSpace(_state.ReviewedStatusPath ?? opt.ReviewedStatusPath))
        {
            return new FileInfo(_state.ReviewedStatusPath ?? opt.ReviewedStatusPath!);
        }

        var root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrWhiteSpace(root))
        {
            root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
        }

        return new FileInfo(Path.Combine(root, "AMS", "validation-viewer", "reviewed-status.json"));
    }
}
