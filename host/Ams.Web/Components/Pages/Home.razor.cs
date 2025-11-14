using System.Linq;
using Ams.Web.Client;
using Ams.Web.Dtos;
using Ams.Web.Requests;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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
    private bool _drawerOpen = true;
    private bool _isPlaying;
    private double _currentPlaybackTime;
    private string? _lastExportMessage;
    private string _exportErrorCode = "MR";
    private string? _exportComment;
    private bool _isExporting;
    private AudioTransport? _audioTransport;
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
            await InvokeAsync(StateHasChanged);
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
            await InvokeAsync(StateHasChanged);
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
        await InvokeAsync(StateHasChanged);

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
            await InvokeAsync(StateHasChanged);
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
            if (_audioTransport is not null)
            {
                var audioElement = _audioTransport.GetAudioElement();
                await JS.InvokeVoidAsync("amsAudio.seekAndPlay", audioElement, sentence.Timing.Start, sentence.Timing.End);
                _isPlaying = true;
            }
        }
        catch (JSException jsEx)
        {
            Snackbar.Add($"Audio playback failed: {jsEx.Message}", Severity.Warning);
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task TogglePlayPause()
    {
        if (_audioTransport is null || _selectedSentence?.Timing is null) return;

        try
        {
            var audioElement = _audioTransport.GetAudioElement();
            if (_isPlaying)
            {
                await JS.InvokeVoidAsync("amsAudio.pause", audioElement);
                _isPlaying = false;
            }
            else
            {
                await JS.InvokeVoidAsync("amsAudio.seekAndPlay", audioElement, _selectedSentence.Timing.Start, _selectedSentence.Timing.End);
                _isPlaying = true;
            }
        }
        catch (JSException jsEx)
        {
            Snackbar.Add($"Playback control failed: {jsEx.Message}", Severity.Warning);
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task StopPlayback()
    {
        if (_audioTransport is null) return;

        try
        {
            var audioElement = _audioTransport.GetAudioElement();
            await JS.InvokeVoidAsync("amsAudio.stop", audioElement);
            _isPlaying = false;
        }
        catch (JSException jsEx)
        {
            Snackbar.Add($"Stop failed: {jsEx.Message}", Severity.Warning);
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task NavigatePrevious()
    {
        if (_selectedChapter is null || _selectedSentence is null) return;

        var sentences = _selectedChapter.Sentences;
        var currentIndex = -1;
        for (int i = 0; i < sentences.Count; i++)
        {
            if (sentences[i] == _selectedSentence)
            {
                currentIndex = i;
                break;
            }
        }
        if (currentIndex > 0)
        {
            await OnSentenceSelected(sentences[currentIndex - 1]);
        }
    }

    private async Task NavigateNext()
    {
        if (_selectedChapter is null || _selectedSentence is null) return;

        var sentences = _selectedChapter.Sentences;
        var currentIndex = -1;
        for (int i = 0; i < sentences.Count; i++)
        {
            if (sentences[i] == _selectedSentence)
            {
                currentIndex = i;
                break;
            }
        }
        if (currentIndex >= 0 && currentIndex < sentences.Count - 1)
        {
            await OnSentenceSelected(sentences[currentIndex + 1]);
        }
    }

    private async Task ExportCurrentSentence()
    {
        if (_selectedSentence?.Timing is null || _selectedChapter is null || _isExporting)
        {
            return;
        }

        try
        {
            _isExporting = true;
            await InvokeAsync(StateHasChanged);

            var request = new ExportSentenceRequest
            {
                ErrorType = string.IsNullOrWhiteSpace(_exportErrorCode) ? null : _exportErrorCode.Trim(),
                Comment = string.IsNullOrWhiteSpace(_exportComment) ? null : _exportComment.Trim()
            };

            var result = await ChapterApi.ExportSentenceAsync(
                _selectedChapter.Id,
                _selectedSentence.Id,
                request,
                _cts.Token).ConfigureAwait(false);

            var fileName = Path.GetFileName(result.SegmentPath);
            _lastExportMessage = $"Row {result.RowNumber} • {fileName}";
            Snackbar.Add($"Exported sentence {_selectedSentence.Id} to {fileName}", Severity.Success);

            _ = Task.Delay(TimeSpan.FromSeconds(5), _cts.Token).ContinueWith(_ =>
            {
                _lastExportMessage = null;
                InvokeAsync(StateHasChanged);
            }, TaskScheduler.Default);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Export failed: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isExporting = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        switch (e.Key.ToLowerInvariant())
        {
            case " ":
            case "spacebar":
                await TogglePlayPause();
                break;

            case "arrowup":
                await NavigatePrevious();
                break;

            case "arrowdown":
                await NavigateNext();
                break;

            case "e":
                if (!e.CtrlKey && !e.AltKey)
                {
                    await ExportCurrentSentence();
                }
                break;

            case "escape":
                await StopPlayback();
                break;
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

    private IReadOnlyList<SentenceDto> SentencesToDisplay =>
        SelectedChapter?.Sentences ?? Array.Empty<SentenceDto>();

    private string WorkspaceDescription =>
        _isWorkspaceLoading
            ? "Loading workspace…"
            : _workspace?.WorkspaceRoot ?? "No workspace configured.";

    private string? AudioSource =>
        _selectedChapter is null
            ? null
            : $"api/chapters/{Uri.EscapeDataString(_selectedChapter.Id)}/audio";

    private string ContentMarginStyle =>
        _drawerOpen ? "margin-left: 280px; transition: margin-left 0.3s ease;" : "margin-left: 0; transition: margin-left 0.3s ease;";

    private bool CanNavigatePrevious
    {
        get
        {
            if (_selectedChapter is null || _selectedSentence is null) return false;
            var index = -1;
            for (int i = 0; i < _selectedChapter.Sentences.Count; i++)
            {
                if (_selectedChapter.Sentences[i] == _selectedSentence)
                {
                    index = i;
                    break;
                }
            }
            return index > 0;
        }
    }

    private bool CanNavigateNext
    {
        get
        {
            if (_selectedChapter is null || _selectedSentence is null) return false;
            var index = -1;
            for (int i = 0; i < _selectedChapter.Sentences.Count; i++)
            {
                if (_selectedChapter.Sentences[i] == _selectedSentence)
                {
                    index = i;
                    break;
                }
            }
            return index >= 0 && index < _selectedChapter.Sentences.Count - 1;
        }
    }

    private static string FormatDuration(double seconds) =>
        TimeSpan.FromSeconds(seconds).ToString(@"hh\:mm\:ss");

    private string GetChapterItemClass(ChapterListItemDto chapter) =>
        chapter.Id == _selectedChapter?.Id ? "mud-selected-item" : string.Empty;

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
