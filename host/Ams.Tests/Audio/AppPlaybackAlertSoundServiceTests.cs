using Ams.Core.Runtime.Audio;
using Ams.Core.Runtime.Book;

namespace Ams.Tests.Audio;

public sealed class AppPlaybackAlertSoundServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly string _settingsPath;
    private readonly string _alertsDirectory;

    public AppPlaybackAlertSoundServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), $"ams-alert-sound-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempRoot);
        _settingsPath = Path.Combine(_tempRoot, "app-audio-settings.json");
        _alertsDirectory = Path.Combine(_tempRoot, "audio", "alerts");
    }

    [Fact]
    public void SetPlaybackErrorAlertSound_CopiesSourceIntoConfiguredAppDataLocation()
    {
        var sourcePath = CreateSourceFile("ding.wav");
        var service = CreateService();

        var setting = service.SetPlaybackErrorAlertSound(sourcePath);

        Assert.True(File.Exists(setting.StoredFilePath));
        Assert.StartsWith(_alertsDirectory, setting.StoredFilePath, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("ding.wav", setting.SourceFileName);

        var persisted = service.GetPlaybackErrorAlertSound();
        Assert.NotNull(persisted);
        Assert.Equal(setting.StoredFilePath, persisted!.StoredFilePath, ignoreCase: true);
    }

    [Fact]
    public void ApplyTo_RegistersDescriptorOnBookAudioContext()
    {
        var sourcePath = CreateSourceFile("focus.wav");
        var service = CreateService();
        var setting = service.SetPlaybackErrorAlertSound(sourcePath);

        var manager = new BookManager(new[]
        {
            new BookDescriptor("book-1", _tempRoot, Array.Empty<ChapterDescriptor>())
        });

        service.ApplyTo(manager.Current.Audio);

        var descriptor = manager.Current.Audio.PlaybackErrorAlertSound;
        Assert.NotNull(descriptor);
        Assert.Equal(setting.StoredFilePath, descriptor!.SourcePath, ignoreCase: true);
    }

    [Fact]
    public void ClearPlaybackErrorAlertSound_RemovesConfigurationAndClearsBookAudioContext()
    {
        var sourcePath = CreateSourceFile("notify.wav");
        var service = CreateService();
        var setting = service.SetPlaybackErrorAlertSound(sourcePath);

        var manager = new BookManager(new[]
        {
            new BookDescriptor("book-2", _tempRoot, Array.Empty<ChapterDescriptor>())
        });

        service.ApplyTo(manager.Current.Audio);
        service.ClearPlaybackErrorAlertSound();
        service.ApplyTo(manager.Current.Audio);

        Assert.Null(service.GetPlaybackErrorAlertSound());
        Assert.Null(manager.Current.Audio.PlaybackErrorAlertSound);
        Assert.False(File.Exists(setting.StoredFilePath));
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempRoot))
            {
                Directory.Delete(_tempRoot, recursive: true);
            }
        }
        catch
        {
            // Best-effort cleanup.
        }
    }

    private AppPlaybackAlertSoundService CreateService()
        => new(_settingsPath, _alertsDirectory);

    private string CreateSourceFile(string fileName)
    {
        var sourceDirectory = Path.Combine(_tempRoot, "source");
        Directory.CreateDirectory(sourceDirectory);
        var sourcePath = Path.Combine(sourceDirectory, fileName);

        // File content does not need to be valid WAV for persistence + descriptor registration tests.
        File.WriteAllBytes(sourcePath, new byte[] { 0x52, 0x49, 0x46, 0x46, 0x01, 0x02, 0x03, 0x04 });
        return sourcePath;
    }
}
