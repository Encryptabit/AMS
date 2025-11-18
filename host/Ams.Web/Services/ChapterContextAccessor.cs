using System.Collections.Concurrent;
using System.IO;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Runtime.Workspace;
using Microsoft.Extensions.Logging;

namespace Ams.Web.Services;

public sealed class ChapterContextAccessor : IDisposable
{
    private readonly IWorkspace _workspace;
    private readonly WorkspaceState _workspaceState;
    private readonly ILogger<ChapterContextAccessor> _logger;
    private readonly ConcurrentDictionary<string, ChapterContextHandle> _handles = new(StringComparer.OrdinalIgnoreCase);

    public ChapterContextAccessor(
        IWorkspace workspace,
        WorkspaceState workspaceState,
        ILogger<ChapterContextAccessor> logger)
    {
        _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
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
        var handle = _workspace.OpenChapter(new ChapterOpenOptions
        {
            HydrateFile = new FileInfo(summary.HydratePath),
            AudioFile = summary.AudioPath is null ? null : new FileInfo(summary.AudioPath),
            ChapterDirectory = new DirectoryInfo(summary.RootPath),
            ChapterId = summary.Id
        });

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
