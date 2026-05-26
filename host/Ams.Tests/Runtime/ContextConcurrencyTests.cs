using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Ams.Core.Runtime.Audio;
using Ams.Core.Runtime.Book;
using Ams.Core.Runtime.Chapter;

namespace Ams.Tests.Runtime;

public sealed class ContextConcurrencyTests : IDisposable
{
    private readonly List<string> _tempDirectories = new();

    [Fact]
    public async Task BookManager_Load_ReturnsOneContextWhenCalledConcurrently()
    {
        var root = CreateTempDirectory();
        var manager = new BookManager(new[]
        {
            new BookDescriptor("book-1", root, Array.Empty<ChapterDescriptor>())
        });

        var contexts = await RunConcurrentlyAsync(64, _ => manager.Load("book-1"));

        Assert.All(contexts, context => Assert.Same(contexts[0], context));
    }

    [Fact]
    public async Task ChapterManager_Load_ReturnsOneContextWhenCalledConcurrently()
    {
        var root = CreateTempDirectory();
        var descriptor = new ChapterDescriptor(
            "chapter-1",
            root,
            new[] { new AudioBufferDescriptor("raw", Path.Combine(root, "chapter-1.wav")) });
        var manager = new BookManager(new[]
        {
            new BookDescriptor("book-1", root, new[] { descriptor })
        });
        var chapters = manager.Current.Chapters;

        var contexts = await RunConcurrentlyAsync(64, _ => chapters.Load("chapter-1"));

        Assert.All(contexts, context => Assert.Same(contexts[0], context));
    }

    [Fact]
    public async Task ChapterManager_CreateContext_CanRegisterDistinctChaptersConcurrently()
    {
        var root = CreateTempDirectory();
        var bookIndexFile = WriteBookIndex(root, 40);
        var manager = new BookManager(new[]
        {
            new BookDescriptor("book-1", root, Array.Empty<ChapterDescriptor>())
        });
        var chapters = manager.Current.Chapters;

        var ids = await RunConcurrentlyAsync(40, index =>
        {
            var chapterNumber = index + 1;
            var chapterId = $"Chapter {chapterNumber}";
            var chapterDirectory = Directory.CreateDirectory(Path.Combine(root, chapterId));
            var audioFile = new FileInfo(Path.Combine(root, $"{chapterId}.wav"));

            using var handle = chapters.CreateContext(ChapterOpenRequest.FromTrusted(
                bookIndexFile,
                audioFile: audioFile,
                chapterDirectory: chapterDirectory,
                chapterId: chapterId));

            return handle.Chapter.Descriptor.ChapterId;
        });

        Assert.Equal(40, ids.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(40, chapters.Count);
    }

    [Fact]
    public async Task AudioBufferContext_Buffer_LoadsOnceWhenReadConcurrently()
    {
        var loads = 0;
        var manager = new AudioBufferManager(
            new[] { new AudioBufferDescriptor("raw", "raw.wav") },
            _ =>
            {
                Interlocked.Increment(ref loads);
                Thread.Sleep(20);
                return null;
            });

        var contexts = await RunConcurrentlyAsync(64, _ =>
        {
            var context = manager.Load("raw");
            var buffer = context.Buffer;
            GC.KeepAlive(buffer);
            return context;
        });

        Assert.All(contexts, context => Assert.Same(contexts[0], context));
        Assert.Equal(1, loads);
    }

    [Fact]
    public async Task SharedParser_CanBeInvokedConcurrentlyWithoutDefaultOptionMutation()
    {
        var hits = 0;
        var root = new RootCommand("test-root");
        var command = new Command("noop");
        command.SetHandler(() => { Interlocked.Increment(ref hits); });
        root.AddCommand(command);
        var parser = new CommandLineBuilder(root).UseDefaults().Build();

        var exitCodes = await RunConcurrentlyAsync(64, _ => parser.InvokeAsync(new[] { "noop" }));

        Assert.All(exitCodes, exitCode => Assert.Equal(0, exitCode));
        Assert.Equal(64, hits);
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
        var directory = Path.Combine(Path.GetTempPath(), $"ams-context-concurrency-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        _tempDirectories.Add(directory);
        return directory;
    }

    private static FileInfo WriteBookIndex(string root, int chapterCount)
    {
        var sections = Enumerable.Range(1, chapterCount)
            .Select(index => new SectionRange(
                index,
                $"Chapter {index}",
                1,
                "chapter",
                index - 1,
                index - 1,
                index - 1,
                index - 1))
            .ToArray();

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
            Sections: sections);

        var path = Path.Combine(root, "book-index.json");
        File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(bookIndex));
        return new FileInfo(path);
    }

    private static async Task<T[]> RunConcurrentlyAsync<T>(int count, Func<int, T> action)
    {
        return await RunConcurrentlyAsync(count, index => Task.FromResult(action(index)));
    }

    private static async Task<T[]> RunConcurrentlyAsync<T>(int count, Func<int, Task<T>> action)
    {
        using var start = new ManualResetEventSlim(false);
        var tasks = Enumerable.Range(0, count)
            .Select(index => Task.Run(async () =>
            {
                start.Wait();
                return await action(index);
            }))
            .ToArray();

        start.Set();
        return await Task.WhenAll(tasks);
    }
}
