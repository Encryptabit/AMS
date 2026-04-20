using Ams.Core.Runtime.Book;

namespace Ams.Core.Runtime.Audio;

/// <summary>
/// Contract for persisting and applying an app-level playback alert sound.
/// Hosts can use this to configure one sound that stays active across books.
/// </summary>
public interface IAppPlaybackAlertSoundService
{
    /// <summary>
    /// Returns the configured playback-error alert sound, if any.
    /// </summary>
    AppPlaybackAlertSoundSettings? GetPlaybackErrorAlertSound();

    /// <summary>
    /// Copies a source sound file into AMS app data and sets it as the playback-error alert sound.
    /// </summary>
    AppPlaybackAlertSoundSettings SetPlaybackErrorAlertSound(string sourcePath);

    /// <summary>
    /// Clears the configured playback-error alert sound.
    /// </summary>
    void ClearPlaybackErrorAlertSound();

    /// <summary>
    /// Applies the persisted playback-error alert sound configuration to a book audio context.
    /// </summary>
    void ApplyTo(BookAudio bookAudio);
}

/// <summary>
/// App-level playback alert sound settings persisted in AppData.
/// </summary>
public sealed record AppPlaybackAlertSoundSettings(
    string StoredFilePath,
    string SourceFileName,
    DateTime UpdatedAtUtc);
