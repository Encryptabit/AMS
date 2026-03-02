using FFmpeg.AutoGen;
using System.Text.RegularExpressions;

namespace Ams.Core.Services.Integrations.FFmpeg;

/// <summary>
/// Ensures FFmpeg global state is initialized exactly once.
/// </summary>
public sealed class FfSession : IDisposable
{
    private static readonly object InitLock = new();
    private const int MaxAncestorProbeDepth = 8;
    private static bool _initialized;
    private static bool _filtersChecked;
    private static bool _filtersAvailable;

    private static readonly string[] RootSearchSuffixes = new[]
    {
        Path.Combine("ExtTools", "ffmpeg", "bin"),
        Path.Combine("ExtTools", "ffmpeg", "binaries"),
        Path.Combine("ExtTools", "ffmpeg")
    };

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
            throw new NotSupportedException(
                "FFmpeg filter graph support (libavfilter) is not available. Install FFmpeg builds that include avfilter and place the DLLs under ExtTools/ffmpeg/bin (or ExtTools/ffmpeg/binaries).");
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
        foreach (var candidate in EnumerateRootPathCandidates())
        {
            if (TrySet(candidate))
            {
                return;
            }
        }
    }

    private static IEnumerable<string> EnumerateRootPathCandidates()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        for (var depth = 0; depth < MaxAncestorProbeDepth && current != null; depth++)
        {
            foreach (var candidate in ExpandRootCandidates(current.FullName))
            {
                var full = Path.GetFullPath(candidate);
                if (seen.Add(full))
                {
                    yield return full;
                }
            }

            current = current.Parent;
        }
    }

    private static IEnumerable<string> ExpandRootCandidates(string anchor)
    {
        // Some distributions place FFmpeg DLLs directly next to the app binaries.
        yield return anchor;

        foreach (var suffix in RootSearchSuffixes)
        {
            yield return Path.Combine(anchor, suffix);
        }
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
        ConfigureAutoGenLibraryMap(normalized);
        return true;
    }

    /// <summary>
    /// Align FFmpeg.AutoGen dynamic-library metadata with what exists on disk.
    ///
    /// FFmpeg.AutoGen 8.0.0 ships a dependency map where avfilter depends on
    /// "postproc", but LibraryVersionMap omits a postproc entry. This can throw
    /// KeyNotFoundException during first avfilter call. We normalize that mapping
    /// based on the discovered native files.
    /// </summary>
    private static void ConfigureAutoGenLibraryMap(string rootPath)
    {
        if (!FunctionResolverBase.LibraryDependenciesMap.TryGetValue("avfilter", out var avfilterDeps))
        {
            return;
        }

        var hasPostproc = HasNativeLibraryPrefix(rootPath, "postproc");
        if (!hasPostproc)
        {
            if (avfilterDeps.Any(dep => dep.Equals("postproc", StringComparison.OrdinalIgnoreCase)))
            {
                FunctionResolverBase.LibraryDependenciesMap["avfilter"] = avfilterDeps
                    .Where(dep => !dep.Equals("postproc", StringComparison.OrdinalIgnoreCase))
                    .ToArray();
            }

            FunctionResolverBase.LibraryDependenciesMap.Remove("postproc");
            ffmpeg.LibraryVersionMap.Remove("postproc");
            return;
        }

        if (ffmpeg.LibraryVersionMap.ContainsKey("postproc"))
        {
            return;
        }

        var postprocMajor = DetectLibraryMajorVersion(rootPath, "postproc");
        if (postprocMajor > 0)
        {
            ffmpeg.LibraryVersionMap["postproc"] = postprocMajor;
            return;
        }

        // Conservative fallback for modern Windows/Linux shared FFmpeg builds.
        ffmpeg.LibraryVersionMap["postproc"] = 59;
    }

    private static bool HasNativeLibraryPrefix(string rootPath, string prefix)
    {
        foreach (var file in EnumerateLibraryFiles(rootPath))
        {
            var name = Path.GetFileName(file);
            if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith($"lib{prefix}", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static int DetectLibraryMajorVersion(string rootPath, string prefix)
    {
        var windowsPattern = new Regex(
            $"^{Regex.Escape(prefix)}-(\\d+)\\.dll$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        var unixPattern = new Regex(
            $"^lib{Regex.Escape(prefix)}\\.so\\.(\\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        foreach (var file in EnumerateLibraryFiles(rootPath))
        {
            var name = Path.GetFileName(file);

            var windowsMatch = windowsPattern.Match(name);
            if (windowsMatch.Success && int.TryParse(windowsMatch.Groups[1].Value, out var winMajor))
            {
                return winMajor;
            }

            var unixMatch = unixPattern.Match(name);
            if (unixMatch.Success && int.TryParse(unixMatch.Groups[1].Value, out var unixMajor))
            {
                return unixMajor;
            }
        }

        return 0;
    }

    private static IEnumerable<string> EnumerateLibraryFiles(string rootPath)
    {
        if (!Directory.Exists(rootPath))
        {
            yield break;
        }

        foreach (var file in Directory.EnumerateFiles(rootPath, "*.*", SearchOption.TopDirectoryOnly))
        {
            yield return file;
        }

        var nestedBin = Path.Combine(rootPath, "bin");
        if (!Directory.Exists(nestedBin))
        {
            yield break;
        }

        foreach (var file in Directory.EnumerateFiles(nestedBin, "*.*", SearchOption.TopDirectoryOnly))
        {
            yield return file;
        }
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

        return
            $"{rootHint} Place FFmpeg shared libraries under 'host/ExtTools/ffmpeg/bin' (preferred) or 'host/Ams.Core/ExtTools/ffmpeg/bin'. " +
            "(Older layouts that use '.../ffmpeg/binaries' are also supported.) Download builds from https://ffmpeg.org or https://www.gyan.dev/ffmpeg/builds/ (Windows) and copy the DLLs into that folder.";
    }

    public void Dispose()
    {
        // Intentionally left empty. FFmpeg is kept alive for process lifetime.
    }
}
