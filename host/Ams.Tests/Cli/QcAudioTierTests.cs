using Ams.Cli.Commands;
using Ams.Cli.Utilities;

namespace Ams.Tests.Cli;

public sealed class QcAudioTierTests
{
    [Fact]
    public void DiscoverDirectoryAudioFiles_NoTier_PreservesFlatDirectoryBehavior()
    {
        using var workspace = TempWorkspace.Create();
        var root = workspace.Root;
        File.WriteAllText(Path.Combine(root.FullName, "001.wav"), "source");
        File.WriteAllText(Path.Combine(root.FullName, "001.filtered.wav"), "filtered");

        var nested = Directory.CreateDirectory(Path.Combine(root.FullName, "nested"));
        File.WriteAllText(Path.Combine(nested.FullName, "002.wav"), "nested");

        var files = QcCommand.DiscoverDirectoryAudioFiles(root)
            .Select(file => file.Name)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.Equal(["001.filtered.wav", "001.wav"], files);
    }

    [Fact]
    public void DiscoverDirectoryTierFiles_FilteredTier_RecursesButSkipsSafe()
    {
        using var workspace = TempWorkspace.Create();
        var root = workspace.Root;

        var chapter = Directory.CreateDirectory(Path.Combine(root.FullName, "001"));
        File.WriteAllText(Path.Combine(chapter.FullName, "001.filtered.wav"), "filtered");

        var safe = Directory.CreateDirectory(Path.Combine(root.FullName, "safe"));
        File.WriteAllText(Path.Combine(safe.FullName, "random.filtered.wav"), "safe filtered");

        var files = QcCommand.DiscoverDirectoryTierFiles(root, AudioTier.Filtered)
            .Select(file => Path.GetRelativePath(root.FullName, file.FullName))
            .ToArray();

        Assert.Equal([Path.Combine("001", "001.filtered.wav")], files);
    }

    [Fact]
    public void DiscoverDirectoryTierFiles_SourceTier_ExcludesVariantsAndNonChapterFolders()
    {
        using var workspace = TempWorkspace.Create();
        var root = workspace.Root;
        File.WriteAllText(Path.Combine(root.FullName, "001.wav"), "source");
        File.WriteAllText(Path.Combine(root.FullName, "001.treated.wav"), "treated");
        File.WriteAllText(Path.Combine(root.FullName, "roomtone.wav"), "roomtone");

        var chapter = Directory.CreateDirectory(Path.Combine(root.FullName, "002"));
        File.WriteAllText(Path.Combine(chapter.FullName, "002.wav"), "chapter source");
        File.WriteAllText(Path.Combine(chapter.FullName, "002.filtered.wav"), "chapter filtered");

        var batch = Directory.CreateDirectory(Path.Combine(root.FullName, "Batch 2"));
        File.WriteAllText(Path.Combine(batch.FullName, "003.wav"), "staged");

        var safe = Directory.CreateDirectory(Path.Combine(root.FullName, "safe"));
        File.WriteAllText(Path.Combine(safe.FullName, "random.wav"), "safe");

        var files = QcCommand.DiscoverDirectoryTierFiles(root, AudioTier.Source)
            .Select(file => Path.GetRelativePath(root.FullName, file.FullName))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.Equal(["001.wav", Path.Combine("002", "002.wav")], files);
    }

    private sealed class TempWorkspace : IDisposable
    {
        private TempWorkspace(DirectoryInfo root)
        {
            Root = root;
        }

        public DirectoryInfo Root { get; }

        public static TempWorkspace Create()
        {
            var root = new DirectoryInfo(Path.Combine(Path.GetTempPath(), $"ams-qc-tier-{Guid.NewGuid():N}"));
            root.Create();
            return new TempWorkspace(root);
        }

        public void Dispose()
        {
            if (Root.Exists)
            {
                Root.Delete(recursive: true);
            }
        }
    }
}
