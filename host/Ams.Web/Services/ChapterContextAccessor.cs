using System.Collections.Concurrent;
using System.IO;
using Ams.Core.Application.Contexts;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;
using Microsoft.Extensions.Logging;

namespace Ams.Web.Services;

public sealed class ChapterContextAccessor : IDisposable
{
    private readonly IChapterContextFactory _contextFactory;
    private readonly WorkspaceState _workspaceState;
    private readonly ILogger<ChapterContextAccessor> _logger;
    private readonly ConcurrentDictionary<string, ChapterContextHandle> _handles = new(StringComparer.OrdinalIgnoreCase);

    public ChapterContextAccessor(
        IChapterContextFactory contextFactory,
        WorkspaceState workspaceState,
        ILogger<ChapterContextAccessor> logger)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _workspaceState = workspaceState ?? throw new ArgumentNullException(nameof(workspaceState));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _workspaceState.Changed += (_, _) => Clear();
    }

    public ChapterContext GetChapter(ChapterSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        return GetHandle(summary).Chapter;
    }

    public BookContext GetBook(ChapterSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        return GetHandle(summary).Book;
    }

    public void Save(ChapterSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        GetHandle(summary).Save();
    }

    private ChapterContextHandle GetHandle(ChapterSummary summary)
    {
        return _handles.GetOrAdd(summary.Id, _ => CreateHandle(summary));
    }

    private ChapterContextHandle CreateHandle(ChapterSummary summary)
    {
        var handle = _contextFactory.Create(
            bookIndexFile: new FileInfo(_workspaceState.BookIndexPath),
            hydrateFile: new FileInfo(summary.HydratePath),
            audioFile: summary.AudioPath is null ? null : new FileInfo(summary.AudioPath),
            chapterDirectory: new DirectoryInfo(summary.RootPath),
            chapterId: summary.Id);

        _logger.LogDebug("ChapterContext created for {Chapter}", summary.Id);
        return handle;
    }

    private void Clear()
    {
        foreach (var handle in _handles.Values)
        {
            handle.Dispose();
        }

        _handles.Clear();
    }

    public void Dispose()
    {
        Clear();
    }
}
