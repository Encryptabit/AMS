using Ams.Core.Common;

namespace Ams.Tests.Common;

public sealed class AmsPathResolverTests : IDisposable
{
    private readonly string? _originalWslDistro = Environment.GetEnvironmentVariable("WSL_DISTRO_NAME");

    [Fact]
    public void TranslatePath_WindowsDrive_ToUnixMount()
    {
        var translated = AmsPathResolver.TranslatePath(@"C:\Books\Chapter 01.wav", AmsPathPlatform.Unix);

        Assert.Equal("/mnt/c/Books/Chapter 01.wav", translated);
    }

    [Fact]
    public void TranslatePath_WslMount_ToWindowsDrive()
    {
        var translated = AmsPathResolver.TranslatePath("/mnt/c/Books/Chapter 01.wav", AmsPathPlatform.Windows);

        Assert.Equal(@"C:\Books\Chapter 01.wav", translated);
    }

    [Fact]
    public void TranslatePath_UnixPath_ToWindows_UsesTranslatorWhenNeeded()
    {
        var translated = AmsPathResolver.TranslatePath(
            "/home/cari/project/audio.wav",
            AmsPathPlatform.Windows,
            _ => @"\\wsl.localhost\Ubuntu\home\cari\project\audio.wav");

        Assert.Equal(@"\\wsl.localhost\Ubuntu\home\cari\project\audio.wav", translated);
    }

    [Fact]
    public void TranslatePath_WslUnc_ToUnix_WhenDistroMatches()
    {
        Environment.SetEnvironmentVariable("WSL_DISTRO_NAME", "Ubuntu");

        var translated = AmsPathResolver.TranslatePath(
            @"\\wsl$\Ubuntu\home\cari\project\audio.wav",
            AmsPathPlatform.Unix);

        Assert.Equal("/home/cari/project/audio.wav", translated);
    }

    [Fact]
    public void NormalizeOptionalPath_TrimQuotesAndWhitespace()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"ams-path-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var normalized = AmsPathResolver.NormalizeOptionalPath($"  \"{tempDir}\"  ");

            Assert.Equal(Path.GetFullPath(tempDir), normalized);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("WSL_DISTRO_NAME", _originalWslDistro);
    }
}
