using Ams.Core.Runtime.Chapter;

namespace Ams.Tests.Runtime;

public sealed class ChapterDiscoveryServiceTests
{
    [Fact]
    public void DiscoverChapters_OrdersUnmatchedWavFilesNaturally()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ams-chapter-discovery-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        try
        {
            Touch(Path.Combine(root, "chapter_10.wav"));
            Touch(Path.Combine(root, "chapter_2.wav"));
            Touch(Path.Combine(root, "chapter_1.wav"));

            var chapters = ChapterDiscoveryService.DiscoverChapters(root);

            Assert.Equal(
                ["chapter_1", "chapter_2", "chapter_10"],
                chapters.Select(chapter => chapter.Stem).ToArray());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static void Touch(string path)
    {
        using var stream = File.Create(path);
    }
}
