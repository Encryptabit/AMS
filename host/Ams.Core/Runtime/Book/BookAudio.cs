using Ams.Core.Artifacts;
using Ams.Core.Processors;

namespace Ams.Core.Runtime.Book;

/// <summary>
/// Manages book-level audio assets (roomtone, etc.) with lazy loading.
/// </summary>
public sealed class BookAudio
{
    private readonly BookContext _book;
    private AudioBuffer? _roomtone;
    private bool _roomtoneLoaded;

    internal BookAudio(BookContext book)
    {
        _book = book ?? throw new ArgumentNullException(nameof(book));
    }

    /// <summary>
    /// Gets the path to the roomtone file for this book.
    /// </summary>
    public string RoomtonePath => _book.ResolveArtifactFile("roomtone.wav").FullName;

    /// <summary>
    /// Gets whether a roomtone file exists for this book.
    /// </summary>
    public bool HasRoomtone => File.Exists(RoomtonePath);

    /// <summary>
    /// Gets the roomtone audio buffer, loading it lazily if needed.
    /// Returns null if no roomtone file exists.
    /// </summary>
    public AudioBuffer? Roomtone
    {
        get
        {
            if (!_roomtoneLoaded)
            {
                _roomtone = LoadRoomtone();
                _roomtoneLoaded = true;
            }

            return _roomtone;
        }
    }

    /// <summary>
    /// Unloads the roomtone buffer to free memory.
    /// </summary>
    public void UnloadRoomtone()
    {
        _roomtone = null;
        _roomtoneLoaded = false;
        Log.Debug("BookAudio unloaded roomtone for {BookId}", _book.Descriptor.BookId);
    }

    private AudioBuffer? LoadRoomtone()
    {
        var path = RoomtonePath;
        if (!File.Exists(path))
        {
            Log.Debug("BookAudio roomtone not found at {Path}", path);
            return null;
        }

        try
        {
            var buffer = AudioProcessor.Decode(path);
            Log.Debug(
                "BookAudio loaded roomtone for {BookId} ({Duration:F2}s, {SampleRate}Hz)",
                _book.Descriptor.BookId,
                buffer.Length / (double)buffer.SampleRate,
                buffer.SampleRate);
            return buffer;
        }
        catch (Exception ex)
        {
            Log.Warn("BookAudio failed to load roomtone from {Path}: {Message}", path, ex.Message);
            return null;
        }
    }
}
