using System;
using System.IO;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace Ams.Core.Services.Integrations.FFmpeg;

/// <summary>
/// Ensures FFmpeg global state is initialized exactly once.
/// </summary>
public sealed class FfSession : IDisposable
{
    private static readonly object InitLock = new();
    private static bool _initialized;
    private static bool _filtersChecked;
    private static bool _filtersAvailable;

    /// <summary>
    /// Ensures FFmpeg has been initialized for the current process.
    /// </summary>
    public static void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        lock (InitLock)
        {
            if (_initialized)
            {
                return;
            }

            TrySetRootPath();

            try
            {
                 ffmpeg.av_log_set_level(ffmpeg.AV_LOG_WARNING);
                FfUtils.ThrowIfError(ffmpeg.avformat_network_init(), "Failed to initialize FFmpeg network stack");
                _initialized = true;
            }
            catch (Exception ex) when (IsBindingException(ex))
            {
                var hint = BuildFailureHint();
                throw new InvalidOperationException(
                    $"FFmpeg native libraries could not be loaded. {hint}", ex);
            }
        }
    }

    public static void EnsureFiltersAvailable()
    {
        EnsureInitialized();
        EnsureFilterProbe();
        if (!_filtersAvailable)
        {
            throw new NotSupportedException("FFmpeg filter graph support (libavfilter) is not available. Install FFmpeg builds that include avfilter and place the DLLs under ExtTools/ffmpeg/bin.");
        }
    }

    public static bool FiltersAvailable
    {
        get
        {
            EnsureInitialized();
            EnsureFilterProbe();
            return _filtersAvailable;
        }
    }

    private static void TrySetRootPath()
    {
        var probable = Path.Combine(AppContext.BaseDirectory, "ExtTools", "ffmpeg", "bin");
        TrySet(probable);
    }


    private static bool TrySet(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var normalized = Path.GetFullPath(path);
        if (!HasNativeLibraries(normalized))
        {
            return false;
        }

        ffmpeg.RootPath = normalized;
        return true;
    }

    private static bool HasNativeLibraries(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return false;
        }

        try
        {
            var required = new[]
            {
                "avcodec",
                "avformat",
                "avutil",
                "avfilter"
            };

            foreach (var file in Directory.EnumerateFiles(directory, "*.*", SearchOption.TopDirectoryOnly))
            {
                var name = Path.GetFileName(file) ?? string.Empty;
                foreach (var prefix in required)
                {
                    if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            // Some builds place DLLs one level deeper (e.g., ./bin)
            foreach (var subDir in Directory.EnumerateDirectories(directory))
            {
                var subName = Path.GetFileName(subDir) ?? string.Empty;
                if (subName.Equals("bin", StringComparison.OrdinalIgnoreCase) && HasNativeLibraries(subDir))
                {
                    return true;
                }
            }
        }
        catch
        {
            // ignore, treat as missing
        }
        return false;
    }

    private static bool IsBindingException(Exception ex) =>
        ex is DllNotFoundException or EntryPointNotFoundException or NotSupportedException;

    private static void EnsureFilterProbe()
    {
        if (_filtersChecked)
        {
            return;
        }

        lock (InitLock)
        {
            if (_filtersChecked)
            {
                return;
            }

            try
            {
                unsafe
                {
                    AVFilterGraph* graph = null;
                    try
                    {
                        graph = ffmpeg.avfilter_graph_alloc();
                        _filtersAvailable = graph != null;
                    }
                    catch (EntryPointNotFoundException)
                    {
                        _filtersAvailable = false;
                    }
                    catch (NotSupportedException)
                    {
                        _filtersAvailable = false;
                    }
                    finally
                    {
                        if (graph != null)
                        {
                            ffmpeg.avfilter_graph_free(&graph);
                        }
                    }
                }
            }
            finally
            {
                _filtersChecked = true;
            }
        }
    }

    private static string BuildFailureHint()
    {
        var rootHint = string.IsNullOrWhiteSpace(ffmpeg.RootPath)
            ? "FFmpeg.AutoGen could not locate native binaries."
            : $"Attempted root path: '{ffmpeg.RootPath}'.";

        return $"{rootHint} Place FFmpeg shared libraries under 'ExtTools/ffmpeg/bin' (relative to the solution root) so they ship with the project. " +
               "Download builds from https://ffmpeg.org or https://www.gyan.dev/ffmpeg/builds/ (Windows) and copy the DLLs into that folder.";
    }

    public void Dispose()
    {
        // Intentionally left empty. FFmpeg is kept alive for process lifetime.
    }
}
