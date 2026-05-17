using Ams.Core.Artifacts;
using Ams.Core.Runtime.Artifacts;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;
using Ams.Core.Runtime.Common;

namespace Ams.Tests.Runtime;

public sealed class RuntimeArtifactLifecycleTests : IDisposable
{
    private readonly List<string> _tempDirectories = new();

    [Fact]
    public void ChapterArtifactAddress_BuildsCanonicalArtifactFile()
    {
        var root = CreateTempDirectory();

        var address = new ChapterArtifactAddress(root, "chapter-01", ".align.tx.json");

        Assert.Equal("chapter-01", address.ChapterId);
        Assert.Equal("align.tx.json", address.Suffix);
        Assert.Equal(Path.Combine(root, "chapter-01.align.tx.json"), address.FullPath);
        Assert.Throws<ArgumentException>(() => new ChapterArtifactAddress(root, "chapter-01", "nested/file.json"));
    }

    [Fact]
    public void FileArtifact_ComputesSha256HashOnceOnFirstAccess()
    {
        var root = CreateTempDirectory();
        var path = Path.Combine(root, "source.txt");
        File.WriteAllText(path, "first");

        var artifact = FileArtifact.FromPath(path);

        var originalHash = artifact.Sha256Hash;
        File.WriteAllText(path, "second");

        Assert.True(artifact.Exists);
        Assert.Equal(6, artifact.Length);
        Assert.Equal(originalHash, artifact.Sha256Hash);
        Assert.NotEqual(originalHash, FileArtifact.FromPath(path).Sha256Hash);
    }

    [Fact]
    public void ChapterOpenRequest_PreservesExplicitRawAudioAndBuildsCanonicalDerivedBuffers()
    {
        var root = CreateTempDirectory();
        var bookIndexFile = WriteBookIndex(root);
        var chapterDirectory = Directory.CreateDirectory(Path.Combine(root, "chapter-01"));
        var explicitAudio = new FileInfo(Path.Combine(root, "source-audio.wav"));
        File.WriteAllBytes(explicitAudio.FullName, [0x52, 0x49, 0x46, 0x46]);
        var manager = new BookManager(new[]
        {
            new BookDescriptor("book-1", root, Array.Empty<ChapterDescriptor>())
        });

        var request = ChapterOpenRequest.FromTrusted(
            bookIndexFile,
            audioFile: explicitAudio,
            chapterDirectory: chapterDirectory,
            chapterId: "chapter-01");

        using var handle = manager.Current.Chapters.CreateContext(request);
        var buffers = handle.Chapter.Descriptor.AudioBuffers.ToDictionary(
            descriptor => descriptor.BufferId,
            StringComparer.OrdinalIgnoreCase);

        Assert.Equal(explicitAudio.FullName, buffers["raw"].Path);
        Assert.Equal(Path.Combine(chapterDirectory.FullName, "chapter-01.treated.wav"), buffers["treated"].Path);
        Assert.Equal(Path.Combine(chapterDirectory.FullName, "chapter-01.corrected.wav"), buffers["corrected"].Path);
        Assert.Equal(Path.Combine(chapterDirectory.FullName, "chapter-01.filtered.wav"), buffers["filtered"].Path);
    }

    [Fact]
    public void DocumentSlot_UsesNamedLifecycleStatesForLoadSaveAndInvalidate()
    {
        var saved = new List<string>();
        var slot = new DocumentSlot<string>(() => null, saved.Add);

        Assert.Equal("not-loaded", slot.StateName);

        Assert.Null(slot.GetValue());
        Assert.Equal("loaded-missing", slot.StateName);
        Assert.False(slot.IsDirty);

        slot.SetValue("draft");
        Assert.Equal("loaded-dirty", slot.StateName);
        Assert.True(slot.IsDirty);

        slot.Save();
        Assert.Equal(["draft"], saved);
        Assert.Equal("loaded-clean", slot.StateName);
        Assert.False(slot.IsDirty);

        slot.Invalidate();
        Assert.Equal("invalidated", slot.StateName);
        Assert.False(slot.IsDirty);
    }

    [Fact]
    public void RuntimeLifetimePolicies_NameCurrentRetainLoadedDefaults()
    {
        Assert.Equal(RuntimeCachePolicy.RetainAllEntries, RuntimeLifetimePolicies.BookContexts.MaxEntries);
        Assert.Equal(RuntimeCachePolicy.RetainAllEntries, RuntimeLifetimePolicies.ChapterContexts.MaxEntries);
        Assert.Equal(RuntimeCachePolicy.RetainAllEntries, RuntimeLifetimePolicies.AudioBuffers.MaxEntries);
        Assert.Null(RuntimeLifetimePolicies.ChapterContexts.TimeToLive);
        Assert.True(RuntimeLifetimePolicies.BookContexts.SaveOnUnload);
        Assert.True(RuntimeLifetimePolicies.ChapterContexts.ReleaseResourcesOnUnload);
        Assert.True(RuntimeLifetimePolicies.DocumentSlots.KeepLoadedAfterRead);
        Assert.True(RuntimeLifetimePolicies.DocumentSlots.SaveDirtyOnOwnerSave);
    }

    public void Dispose()
    {
        foreach (var directory in _tempDirectories)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }
            catch
            {
                // Best effort cleanup only.
            }
        }
    }

    private string CreateTempDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"ams-runtime-artifacts-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        _tempDirectories.Add(directory);
        return directory;
    }

    private static FileInfo WriteBookIndex(string root)
    {
        var bookIndex = new BookIndex(
            SourceFile: Path.Combine(root, "book.txt"),
            SourceFileHash: "test",
            IndexedAt: DateTime.UtcNow,
            Title: "Test Book",
            Author: null,
            Totals: new BookTotals(0, 0, 0, 0),
            Words: Array.Empty<BookWord>(),
            Sentences: Array.Empty<SentenceRange>(),
            Paragraphs: Array.Empty<ParagraphRange>(),
            Sections: new[]
            {
                new SectionRange(1, "chapter-01", 1, "chapter", 0, 0, 0, 0)
            });

        var path = Path.Combine(root, "book-index.json");
        File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(bookIndex));
        return new FileInfo(path);
    }
}
