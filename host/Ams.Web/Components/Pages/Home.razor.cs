using System.Linq;
using Ams.Web.Client;
using Ams.Web.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace Ams.Web.Components.Pages;

public partial class Home : ComponentBase, IAsyncDisposable
{
    [Inject] private WorkspaceApiClient WorkspaceApi { get; set; } = default!;
    [Inject] private ChapterApiClient ChapterApi { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private WorkspaceStateDto? _workspace;
    private IReadOnlyList<ChapterListItemDto> _chapters = Array.Empty<ChapterListItemDto>();
    private ChapterDetailDto? _selectedChapter;
    private SentenceDto? _selectedSentence;
    private bool _isWorkspaceLoading = true;
    private bool _isChaptersLoading = true;
    private bool _isChapterLoading;
    private string? _chapterFilter;
    private ElementReference _audioRef;
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadWorkspaceAsync(_cts.Token);
        await LoadChaptersAsync(_cts.Token);
    }

    private async Task LoadWorkspaceAsync(CancellationToken cancellationToken)
    {
        try
        {
            _isWorkspaceLoading = true;
            _workspace = await WorkspaceApi.GetAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to load workspace: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isWorkspaceLoading = false;
            InvokeAsync(StateHasChanged);
        }
    }

    private async Task LoadChaptersAsync(CancellationToken cancellationToken)
    {
        try
        {
            _isChaptersLoading = true;
            _chapters = await ChapterApi.GetChaptersAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to load chapters: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isChaptersLoading = false;
            InvokeAsync(StateHasChanged);
        }
    }

    private async Task SelectChapterAsync(ChapterListItemDto chapter)
    {
        if (_selectedChapter?.Id == chapter.Id)
        {
            return;
        }

        _isChapterLoading = true;
        _selectedChapter = null;
        _selectedSentence = null;
        InvokeAsync(StateHasChanged);

        try
        {
            var detail = await ChapterApi.GetChapterAsync(chapter.Id, _cts.Token).ConfigureAwait(false);
            if (detail is null)
            {
                Snackbar.Add($"Chapter '{chapter.Name}' could not be loaded.", Severity.Warning);
                return;
            }

            _selectedChapter = detail;
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Chapter load failed: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isChapterLoading = false;
            InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnSentenceSelected(SentenceDto sentence)
    {
        _selectedSentence = sentence;
        if (sentence.Timing is null || _selectedChapter is null)
        {
            return;
        }

        try
        {
            await JS.InvokeVoidAsync("amsAudio.seekAndPlay", _audioRef, sentence.Timing.Start, sentence.Timing.End);
        }
        catch (JSException jsEx)
        {
            Snackbar.Add($"Audio playback failed: {jsEx.Message}", Severity.Warning);
        }
    }

    private IReadOnlyList<ChapterListItemDto> FilteredChapters
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_chapterFilter))
            {
                return _chapters;
            }

            var filter = _chapterFilter.Trim();
            return _chapters
                .Where(c => c.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    private ChapterDetailDto? SelectedChapter => _selectedChapter;

    private IEnumerable<SentenceDto> SentencesToDisplay =>
        SelectedChapter?.Sentences ?? Array.Empty<SentenceDto>();

    private string WorkspaceDescription =>
        _isWorkspaceLoading
            ? "Loading workspace…"
            : _workspace?.WorkspaceRoot ?? "No workspace configured.";

    private string? AudioSource =>
        _selectedChapter is null
            ? null
            : $"api/chapters/{Uri.EscapeDataString(_selectedChapter.Id)}/audio";

    private static string FormatDuration(double seconds) =>
        TimeSpan.FromSeconds(seconds).ToString(@"hh\:mm\:ss");

    private static string FormatTime(double? seconds) =>
        seconds is null ? "--:--" : TimeSpan.FromSeconds(seconds.Value).ToString(@"mm\:ss\.fff");

    private string GetChapterItemClass(ChapterListItemDto chapter) =>
        chapter.Id == _selectedChapter?.Id ? "mud-selected-item" : string.Empty;

    private string GetSentenceRowClass(SentenceDto sentence) =>
        sentence == _selectedSentence ? "ams-sentence-row selected" : "ams-sentence-row";

    public ValueTask DisposeAsync()
    {
        if (!_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }

        _cts.Dispose();
        return ValueTask.CompletedTask;
    }
}
