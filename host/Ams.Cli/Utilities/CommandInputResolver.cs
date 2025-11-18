using Ams.Cli.Repl;
using Ams.Cli.Workspace;
using Ams.Core.Runtime.Workspace;

namespace Ams.Cli.Utilities;

internal static class CommandInputResolver
{
    public static FileInfo RequireAudio(FileInfo? provided)
    {
        if (provided is not null)
        {
            return provided;
        }

        var context = ReplContext.Current;
        if (context?.ActiveChapter is not null)
        {
            return context.ActiveChapter;
        }

        throw new InvalidOperationException("Audio file is required. Provide --audio or select a chapter with 'use'.");
    }

    public static FileInfo ResolveOutput(FileInfo? provided, string suffix)
    {
        if (provided is not null)
        {
            return provided;
        }

        var context = ReplContext.Current;
        if (context?.ActiveChapterStem is null)
        {
            throw new InvalidOperationException("Cannot derive output path without an active chapter. Provide --out explicitly.");
        }

        return context.ResolveChapterFile(suffix, mustExist: false);
    }

    public static FileInfo ResolveChapterArtifact(FileInfo? provided, string suffix, bool mustExist = true)
    {
        if (provided is not null)
        {
            return provided;
        }

        var context = ReplContext.Current;
        if (context is null)
        {
            throw new InvalidOperationException("Repl context missing. Provide explicit path.");
        }

        return context.ResolveChapterFile(suffix, mustExist);
    }

    public static FileInfo? TryResolveChapterArtifact(FileInfo? provided, string suffix, bool mustExist = true)
    {
        if (provided is not null)
        {
            return provided;
        }

        var context = ReplContext.Current;
        if (context?.ActiveChapterStem is null)
        {
            return null;
        }

        var candidate = context.ResolveChapterFile(suffix, mustExist: false);
        if (mustExist && !candidate.Exists)
        {
            return null;
        }

        return candidate;
    }

    public static FileInfo ResolveBookIndex(FileInfo? provided, bool mustExist = true)
    {
        if (provided is not null)
        {
            return provided;
        }

        var context = ReplContext.Current;
        if (context is not null)
        {
            return context.ResolveBookIndex(mustExist);
        }

        var fallback = Path.Combine(Directory.GetCurrentDirectory(), "book-index.json");
        if (mustExist && !File.Exists(fallback))
        {
            throw new FileNotFoundException("Book index not found in working directory. Provide --book-index.", fallback);
        }

        return new FileInfo(fallback);
    }

    public static FileInfo ResolveBookSource(FileInfo? provided)
    {
        if (provided is not null)
        {
            return provided;
        }

        var searchRoot = ReplContext.Current?.WorkingDirectory ?? Directory.GetCurrentDirectory();
        var patterns = new[] { "*.docx", "*.txt", "*.md", "*.rtf", "*.pdf" };
        foreach (var pattern in patterns)
        {
            var match = Directory.EnumerateFiles(searchRoot, pattern, SearchOption.TopDirectoryOnly)
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
            if (match is not null)
            {
                return new FileInfo(match);
            }
        }

        throw new InvalidOperationException("No manuscript file found in working directory. Provide --book or add a DOCX/TXT/MD/RTF file.");
    }

    public static DirectoryInfo ResolveDirectory(DirectoryInfo? provided)
    {
        if (provided is not null)
        {
            return provided;
        }

        var context = ReplContext.Current;
        return context is not null
            ? new DirectoryInfo(context.WorkingDirectory)
            : new DirectoryInfo(Directory.GetCurrentDirectory());
    }

    public static string ResolveChapterId(string? provided)
    {
        if (!string.IsNullOrWhiteSpace(provided))
        {
            return provided;
        }

        var context = ReplContext.Current;
        if (context?.ActiveChapterStem is not null)
        {
            return context.ActiveChapterStem;
        }

        throw new InvalidOperationException("Chapter identifier required. Provide --chapter-id or select a chapter.");
    }

    public static IWorkspace ResolveWorkspace(FileInfo? bookIndexFile = null)
    {
        if (ReplContext.Current is { } state)
        {
            return state.Workspace;
        }

        if (bookIndexFile is null)
        {
            throw new InvalidOperationException("Book index must be specified when not running inside the REPL workspace.");
        }

        var root = bookIndexFile.Directory?.FullName
                   ?? throw new InvalidOperationException("Book index path must have a parent directory.");
        return new CliWorkspace(root);
    }
}
