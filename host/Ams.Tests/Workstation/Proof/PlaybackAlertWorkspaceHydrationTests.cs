using Ams.Core.Runtime.Audio;
using Ams.Workstation.Server.Services;

namespace Ams.Tests.Workstation.Proof;

public sealed class PlaybackAlertWorkspaceHydrationTests : IDisposable
{
    private readonly string _tempRoot;

    public PlaybackAlertWorkspaceHydrationTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), $"ams-playback-alert-workspace-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempRoot);
    }

    [Fact]
    public void SetWorkingDirectory_LoadsPersistedPlaybackAlertIntoBookAudio()
    {
        var workspaceRoot = Path.Combine(_tempRoot, "workspace");
        Directory.CreateDirectory(workspaceRoot);
        File.WriteAllText(Path.Combine(workspaceRoot, "book-index.json"), "{}");

        var settingsPath = Path.Combine(_tempRoot, "app-audio-settings.json");
        var alertsDirectory = Path.Combine(_tempRoot, "audio", "alerts");
        var sourcePath = CreateSourceFile("alert.wav");

        var playbackAlertService = new AppPlaybackAlertSoundService(settingsPath, alertsDirectory);
        var setting = playbackAlertService.SetPlaybackErrorAlertSound(sourcePath);

        using var workspace = new BlazorWorkspace(
            stateFilePath: Path.Combine(_tempRoot, "workstation-state.json"),
            loadPersistedState: false,
            playbackAlertSoundService: playbackAlertService);

        var initialized = workspace.SetWorkingDirectory(workspaceRoot);

        Assert.True(initialized);
        var descriptor = workspace.Book.Audio.PlaybackErrorAlertSound;
        Assert.NotNull(descriptor);
        Assert.Equal(setting.StoredFilePath, descriptor!.SourcePath, ignoreCase: true);
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

    private string CreateSourceFile(string fileName)
    {
        var sourceDirectory = Path.Combine(_tempRoot, "source");
        Directory.CreateDirectory(sourceDirectory);
        var sourcePath = Path.Combine(sourceDirectory, fileName);
        File.WriteAllBytes(sourcePath, new byte[] { 0x52, 0x49, 0x46, 0x46, 0x05, 0x06, 0x07, 0x08 });
        return sourcePath;
    }
}
