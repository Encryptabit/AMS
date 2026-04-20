using System.Text.Json;
using Ams.Core.Common;
using Ams.Core.Runtime.Book;

namespace Ams.Core.Runtime.Audio;

/// <summary>
/// Persists an app-level playback-error alert sound in AppData and applies it to <see cref="BookAudio"/>.
/// </summary>
public sealed class AppPlaybackAlertSoundService : IAppPlaybackAlertSoundService
{
    private const string PlaybackErrorAlertFileStem = "playback-error-alert";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly object _sync = new();
    private readonly string _settingsFilePath;
    private readonly string _alertSoundDirectory;
    private AppPlaybackAlertSoundSettings? _playbackErrorAlertSound;
    private bool _isLoaded;

    public AppPlaybackAlertSoundService(string? settingsFilePath = null, string? alertSoundDirectory = null)
    {
        _settingsFilePath = string.IsNullOrWhiteSpace(settingsFilePath)
            ? AmsAppDataPaths.Resolve("app-audio-settings.json")
            : Path.GetFullPath(settingsFilePath);

        _alertSoundDirectory = string.IsNullOrWhiteSpace(alertSoundDirectory)
            ? AmsAppDataPaths.Resolve("audio", "alerts")
            : Path.GetFullPath(alertSoundDirectory);
    }

    public AppPlaybackAlertSoundSettings? GetPlaybackErrorAlertSound()
    {
        lock (_sync)
        {
            EnsureLoaded();
            return _playbackErrorAlertSound;
        }
    }

    public AppPlaybackAlertSoundSettings SetPlaybackErrorAlertSound(string sourcePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);

        lock (_sync)
        {
            EnsureLoaded();

            var sourceFile = new FileInfo(sourcePath.Trim());
            if (!sourceFile.Exists)
            {
                throw new FileNotFoundException($"Playback alert sound file not found: {sourcePath}", sourcePath);
            }

            Directory.CreateDirectory(_alertSoundDirectory);

            var extension = string.IsNullOrWhiteSpace(sourceFile.Extension)
                ? ".wav"
                : sourceFile.Extension.ToLowerInvariant();

            var destinationPath = Path.Combine(_alertSoundDirectory, $"{PlaybackErrorAlertFileStem}{extension}");
            File.Copy(sourceFile.FullName, destinationPath, overwrite: true);
            CleanupStaleAlertCopies(keepPath: destinationPath);

            _playbackErrorAlertSound = new AppPlaybackAlertSoundSettings(
                StoredFilePath: destinationPath,
                SourceFileName: sourceFile.Name,
                UpdatedAtUtc: DateTime.UtcNow);

            PersistLocked();

            Log.Info(
                "Configured playback error alert sound at {StoredPath} from source {SourcePath}",
                destinationPath,
                sourceFile.FullName);

            return _playbackErrorAlertSound;
        }
    }

    public void ClearPlaybackErrorAlertSound()
    {
        lock (_sync)
        {
            EnsureLoaded();

            if (_playbackErrorAlertSound is not null)
            {
                DeleteIfExists(_playbackErrorAlertSound.StoredFilePath);
            }

            CleanupStaleAlertCopies(keepPath: null);
            _playbackErrorAlertSound = null;
            PersistLocked();
        }
    }

    public void ApplyTo(BookAudio bookAudio)
    {
        ArgumentNullException.ThrowIfNull(bookAudio);

        lock (_sync)
        {
            EnsureLoaded();

            if (_playbackErrorAlertSound is null)
            {
                bookAudio.ClearPlaybackErrorAlertSound();
                return;
            }

            if (!File.Exists(_playbackErrorAlertSound.StoredFilePath))
            {
                Log.Warn(
                    "Configured playback alert sound is missing at {Path}; clearing persisted setting.",
                    _playbackErrorAlertSound.StoredFilePath);
                _playbackErrorAlertSound = null;
                PersistLocked();
                bookAudio.ClearPlaybackErrorAlertSound();
                return;
            }

            bookAudio.RegisterPlaybackErrorAlertSound(_playbackErrorAlertSound.StoredFilePath);
        }
    }

    private void EnsureLoaded()
    {
        if (_isLoaded)
        {
            return;
        }

        _playbackErrorAlertSound = LoadFromDisk();
        _isLoaded = true;
    }

    private AppPlaybackAlertSoundSettings? LoadFromDisk()
    {
        if (!File.Exists(_settingsFilePath))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(_settingsFilePath);
            var document = JsonSerializer.Deserialize<AppPlaybackAlertSoundDocument>(json, JsonOptions);
            var raw = document?.PlaybackErrorAlertSound;
            if (raw is null || string.IsNullOrWhiteSpace(raw.StoredFilePath))
            {
                return null;
            }

            var normalizedPath = Path.GetFullPath(raw.StoredFilePath);
            if (!File.Exists(normalizedPath))
            {
                Log.Warn(
                    "Playback alert settings reference missing file {Path}; setting will be ignored.",
                    normalizedPath);
                return null;
            }

            var sourceFileName = string.IsNullOrWhiteSpace(raw.SourceFileName)
                ? Path.GetFileName(normalizedPath)
                : raw.SourceFileName;

            return new AppPlaybackAlertSoundSettings(
                StoredFilePath: normalizedPath,
                SourceFileName: sourceFileName,
                UpdatedAtUtc: raw.UpdatedAtUtc);
        }
        catch (Exception ex)
        {
            Log.Warn(
                "Failed to load app playback alert settings from {Path}: {Message}",
                _settingsFilePath,
                ex.Message);
            return null;
        }
    }

    private void PersistLocked()
    {
        try
        {
            var settingsDirectory = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrWhiteSpace(settingsDirectory))
            {
                Directory.CreateDirectory(settingsDirectory);
            }

            var document = new AppPlaybackAlertSoundDocument
            {
                PlaybackErrorAlertSound = _playbackErrorAlertSound
            };

            var json = JsonSerializer.Serialize(document, JsonOptions);
            File.WriteAllText(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            Log.Warn(
                "Failed to persist app playback alert settings to {Path}: {Message}",
                _settingsFilePath,
                ex.Message);
        }
    }

    private void CleanupStaleAlertCopies(string? keepPath)
    {
        if (!Directory.Exists(_alertSoundDirectory))
        {
            return;
        }

        foreach (var candidatePath in Directory.EnumerateFiles(_alertSoundDirectory, $"{PlaybackErrorAlertFileStem}.*"))
        {
            if (!string.IsNullOrWhiteSpace(keepPath)
                && string.Equals(candidatePath, keepPath, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            DeleteIfExists(candidatePath);
        }
    }

    private static void DeleteIfExists(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Best effort cleanup.
        }
    }

    private sealed class AppPlaybackAlertSoundDocument
    {
        public AppPlaybackAlertSoundSettings? PlaybackErrorAlertSound { get; set; }
    }
}
