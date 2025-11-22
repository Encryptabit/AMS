using System.IO;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;

namespace Ams.Core.Runtime.Workspace;

public interface IWorkspace
{
    /// <summary>
    /// Root directory that represents this workspace (typically the book folder).
    /// </summary>
    string RootPath { get; }

    /// <summary>
    /// The long-lived book context for this workspace.
    /// </summary>
    BookContext Book { get; }

    /// <summary>
    /// Convenience accessor for the chapter manager.
    /// </summary>
    ChapterManager Chapters => Book.Chapters;

    /// <summary>
    /// Opens (or creates) a chapter context according to the supplied options.
    /// Workspaces are responsible for filling in any missing defaults (e.g.,
    /// book-index path, chapter directory) that are specific to the host.
    /// </summary>
    ChapterContextHandle OpenChapter(ChapterOpenOptions options);
}

public sealed record ChapterOpenOptions
{
    public FileInfo? BookIndexFile { get; init; }
    public FileInfo? AsrFile { get; init; }
    public FileInfo? TranscriptFile { get; init; }
    public FileInfo? HydrateFile { get; init; }
    public FileInfo? AudioFile { get; init; }
    public DirectoryInfo? ChapterDirectory { get; init; }
    public string? ChapterId { get; init; }
    public bool ReloadBookIndex { get; init; }
}