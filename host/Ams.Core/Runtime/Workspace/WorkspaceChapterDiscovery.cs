using System.IO;
using Ams.Core.Runtime.Book;

namespace Ams.Core.Runtime.Workspace;

/// <summary>
/// Helper to build chapter descriptors from an on-disk workspace root.
/// Lives in Ams.Core so consumer apps only pass the root path; all file-system
/// knowledge stays within the core runtime.
/// </summary>
public static class WorkspaceChapterDiscovery
{
    public static IReadOnlyList<ChapterDescriptor> Discover(string bookRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bookRoot);

        var root = new DirectoryInfo(bookRoot);
        if (!root.Exists)
        {
            throw new DirectoryNotFoundException(bookRoot);
        }

        var descriptors = new List<ChapterDescriptor>();
        foreach (var dir in root.EnumerateDirectories())
        {
            var chapterId = dir.Name;

            string RawPath(string name) => Path.Combine(dir.FullName, name);
            string RootPath(string name) => Path.Combine(root.FullName, name);

            // Prefer chapter-local raw, else book-root raw
            var rawFileName = $"{chapterId}.wav";
            var rawPath = File.Exists(RawPath(rawFileName))
                ? RawPath(rawFileName)
                : RootPath(rawFileName);

            var buffers = new List<AudioBufferDescriptor>
            {
                new("raw", rawPath),
                new("treated", Path.Combine(dir.FullName, $"{chapterId}.treated.wav")),
                new("filtered", Path.Combine(dir.FullName, $"{chapterId}.filtered.wav"))
            };

            descriptors.Add(new ChapterDescriptor(chapterId, dir.FullName, buffers));
        }

        return descriptors;
    }
}
