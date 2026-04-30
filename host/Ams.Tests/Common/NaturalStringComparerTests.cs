using Ams.Core.Common;

namespace Ams.Tests.Common;

public class NaturalStringComparerTests
{
    [Fact]
    public void SortIgnoreCase_OrdersEmbeddedNumbersByNumericValue()
    {
        var sorted = new[] { "Chapter 10", "chapter 2", "Chapter 1" }
            .OrderBy(value => value, NaturalStringComparer.SortIgnoreCase)
            .ToArray();

        Assert.Equal(["Chapter 1", "chapter 2", "Chapter 10"], sorted);
    }

    [Fact]
    public void FileNameWithoutExtensionIgnoreCase_OrdersWavNamesByNumericValue()
    {
        var root = new DirectoryInfo(Path.Combine(Path.GetTempPath(), $"ams-natural-sort-{Guid.NewGuid():N}"));
        try
        {
            var sorted = new[]
                {
                    new FileInfo(Path.Combine(root.FullName, "chapter_10.wav")),
                    new FileInfo(Path.Combine(root.FullName, "chapter_2.wav")),
                    new FileInfo(Path.Combine(root.FullName, "chapter_1.wav"))
                }
                .OrderBy(file => file, NaturalStringComparer.FileNameWithoutExtensionIgnoreCase)
                .Select(file => file.Name)
                .ToArray();

            Assert.Equal(["chapter_1.wav", "chapter_2.wav", "chapter_10.wav"], sorted);
        }
        finally
        {
            if (root.Exists)
            {
                root.Delete(recursive: true);
            }
        }
    }
}
