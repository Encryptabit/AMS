using Ams.Cli.Commands;
using Ams.Cli.Utilities;

namespace Ams.Tests.Cli;

public sealed class PipelineStageAudioTierTests
{
    [Fact]
    public void DiscoverStageFiles_SourceTier_ReturnsOnlySourceWavs()
    {
        using var workspace = TempWorkspace.Create();
        var root = workspace.Root;
        var destination = Directory.CreateDirectory(Path.Combine(root.FullName, "Batch 2"));

        File.WriteAllText(Path.Combine(root.FullName, "001.wav"), "source");
        File.WriteAllText(Path.Combine(root.FullName, "001.treated.wav"), "treated");
        File.WriteAllText(Path.Combine(root.FullName, "001.filtered.wav"), "filtered");
        File.WriteAllText(Path.Combine(destination.FullName, "staged.wav"), "staged");

        var chapterDir = Directory.CreateDirectory(Path.Combine(root.FullName, "002"));
        File.WriteAllText(Path.Combine(chapterDir.FullName, "002.wav"), "chapter source");
        File.WriteAllText(Path.Combine(chapterDir.FullName, "002.treated.wav"), "chapter treated");

        var safeDir = Directory.CreateDirectory(Path.Combine(root.FullName, "safe"));
        File.WriteAllText(Path.Combine(safeDir.FullName, "roomtone.wav"), "roomtone");
        File.WriteAllText(Path.Combine(safeDir.FullName, "random.wav"), "random");

        var files = PipelineCommand.DiscoverStageFiles(
                root,
                NormalizeDirectoryPath(destination.FullName),
                AudioTier.Source)
            .Select(file => Path.GetRelativePath(root.FullName, file.FullName))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.Equal(["001.wav", Path.Combine("002", "002.wav")], files);
    }

    [Fact]
    public void DiscoverStageFiles_FilteredTier_ReturnsFilteredArtifacts()
    {
        using var workspace = TempWorkspace.Create();
        var root = workspace.Root;
        var destination = Directory.CreateDirectory(Path.Combine(root.FullName, "Batch 2"));

        File.WriteAllText(Path.Combine(root.FullName, "001.wav"), "source");
        File.WriteAllText(Path.Combine(root.FullName, "001.filtered.wav"), "filtered");
        File.WriteAllText(Path.Combine(root.FullName, "001.dsp.filtered.wav"), "old filtered");
        File.WriteAllText(Path.Combine(destination.FullName, "001.filtered.wav"), "already staged");

        var safeDir = Directory.CreateDirectory(Path.Combine(root.FullName, "safe"));
        File.WriteAllText(Path.Combine(safeDir.FullName, "roomtone.wav"), "roomtone");
        File.WriteAllText(Path.Combine(safeDir.FullName, "random.filtered.wav"), "safe filtered");

        var files = PipelineCommand.DiscoverStageFiles(
                root,
                NormalizeDirectoryPath(destination.FullName),
                AudioTier.Filtered)
            .Select(file => file.Name)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.Equal(["001.dsp.filtered.wav", "001.filtered.wav"], files);
    }

    private static string NormalizeDirectoryPath(string path)
    {
        var full = Path.GetFullPath(path);
        return full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
               + Path.DirectorySeparatorChar;
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
            var root = new DirectoryInfo(Path.Combine(Path.GetTempPath(), $"ams-stage-tier-{Guid.NewGuid():N}"));
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
