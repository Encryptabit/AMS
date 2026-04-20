using System;
using System.Diagnostics;
using System.IO;

namespace Ams.Core.Common;

public enum AmsPathPlatform
{
    Current,
    Windows,
    Unix
}

/// <summary>
/// Normalizes user-supplied paths across Windows and Unix/WSL environments.
/// Allows users to paste Windows paths into WSL-hosted AMS surfaces and WSL paths
/// into Windows-hosted AMS surfaces without manual conversion.
/// </summary>
public static class AmsPathResolver
{
    private const string WslMountPrefix = "/mnt/";
    private const string WslUncPrefix = @"\\wsl$\";
    private const string WslLocalhostUncPrefix = @"\\wsl.localhost\";

    public static string? NormalizeOptionalPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var trimmed = TrimPath(path);
        return string.IsNullOrWhiteSpace(trimmed) ? null : NormalizePath(trimmed);
    }

    public static string NormalizePath(string path)
    {
        var trimmed = TrimPath(path);
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Path cannot be empty.", nameof(path));
        }

        var translated = TranslatePath(trimmed, AmsPathPlatform.Current);
        try
        {
            return Path.GetFullPath(translated);
        }
        catch
        {
            return translated;
        }
    }

    public static string NormalizePath(FileSystemInfo path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return NormalizePath(path.ToString());
    }

    public static FileInfo NormalizeFile(FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(file);
        return new FileInfo(NormalizePath(file));
    }

    public static DirectoryInfo NormalizeDirectory(DirectoryInfo directory)
    {
        ArgumentNullException.ThrowIfNull(directory);
        return new DirectoryInfo(NormalizePath(directory));
    }

    public static string TranslatePath(
        string path,
        AmsPathPlatform targetPlatform,
        Func<string, string?>? unixToWindowsTranslator = null)
    {
        var trimmed = TrimPath(path);
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Path cannot be empty.", nameof(path));
        }

        var targetIsWindows = targetPlatform switch
        {
            AmsPathPlatform.Current => OperatingSystem.IsWindows(),
            AmsPathPlatform.Windows => true,
            AmsPathPlatform.Unix => false,
            _ => OperatingSystem.IsWindows()
        };

        return targetIsWindows
            ? TranslateToWindows(trimmed, unixToWindowsTranslator)
            : TranslateToUnix(trimmed);
    }

    private static string TranslateToWindows(string path, Func<string, string?>? unixToWindowsTranslator)
    {
        if (TryTranslateWslDrivePathToWindows(path, out var translated))
        {
            return NormalizeWindowsSeparators(translated);
        }

        if (LooksLikeUnixAbsolutePath(path))
        {
            var viaTranslator = unixToWindowsTranslator?.Invoke(path) ?? TryTranslateUnixPathToWindowsViaWsl(path);
            if (!string.IsNullOrWhiteSpace(viaTranslator))
            {
                return NormalizeWindowsSeparators(viaTranslator.Trim());
            }
        }

        return NormalizeWindowsSeparators(path);
    }

    private static string TranslateToUnix(string path)
    {
        if (TryTranslateWindowsDrivePathToUnix(path, out var translated))
        {
            return translated;
        }

        if (TryTranslateWslUncPathToUnix(path, out translated))
        {
            return translated;
        }

        return path.Replace('\\', '/');
    }

    private static string TrimPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return path.Trim().Trim('"', '\'').Trim();
    }

    private static bool LooksLikeUnixAbsolutePath(string path)
        => path.StartsWith("/", StringComparison.Ordinal);

    private static bool TryTranslateWindowsDrivePathToUnix(string path, out string translated)
    {
        translated = string.Empty;
        if (!LooksLikeWindowsDrivePath(path))
        {
            return false;
        }

        var driveLetter = char.ToLowerInvariant(path[0]);
        var remainder = path.Length > 2 ? path[2..].TrimStart('\\', '/') : string.Empty;
        remainder = remainder.Replace('\\', '/');

        translated = string.IsNullOrEmpty(remainder)
            ? $"/mnt/{driveLetter}"
            : $"/mnt/{driveLetter}/{remainder}";
        return true;
    }

    private static bool TryTranslateWslDrivePathToWindows(string path, out string translated)
    {
        translated = string.Empty;
        if (!path.StartsWith(WslMountPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (path.Length < WslMountPrefix.Length + 1)
        {
            return false;
        }

        var driveLetter = path[WslMountPrefix.Length];
        if (!char.IsLetter(driveLetter))
        {
            return false;
        }

        var afterDriveIndex = WslMountPrefix.Length + 1;
        if (path.Length > afterDriveIndex && path[afterDriveIndex] != '/')
        {
            return false;
        }

        var remainder = path.Length > afterDriveIndex + 1
            ? path[(afterDriveIndex + 1)..].Replace('/', '\\')
            : string.Empty;

        translated = string.IsNullOrEmpty(remainder)
            ? $"{char.ToUpperInvariant(driveLetter)}:\\"
            : $"{char.ToUpperInvariant(driveLetter)}:\\{remainder}";
        return true;
    }

    private static bool TryTranslateWslUncPathToUnix(string path, out string translated)
    {
        translated = string.Empty;

        var normalized = path.Replace('/', '\\');
        var prefix = normalized.StartsWith(WslUncPrefix, StringComparison.OrdinalIgnoreCase)
            ? WslUncPrefix
            : normalized.StartsWith(WslLocalhostUncPrefix, StringComparison.OrdinalIgnoreCase)
                ? WslLocalhostUncPrefix
                : null;

        if (prefix is null)
        {
            return false;
        }

        var remaining = normalized[prefix.Length..];
        var separatorIndex = remaining.IndexOf('\\');
        if (separatorIndex < 0)
        {
            return false;
        }

        var distro = remaining[..separatorIndex];
        var currentDistro = Environment.GetEnvironmentVariable("WSL_DISTRO_NAME");
        if (string.IsNullOrWhiteSpace(currentDistro)
            || !string.Equals(distro, currentDistro, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var unixRemainder = remaining[(separatorIndex + 1)..].Replace('\\', '/').TrimStart('/');
        translated = "/" + unixRemainder;
        return true;
    }

    private static bool LooksLikeWindowsDrivePath(string path)
        => path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':';

    private static string NormalizeWindowsSeparators(string path)
        => path.Replace('/', '\\');

    private static string? TryTranslateUnixPathToWindowsViaWsl(string path)
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.StartInfo.ArgumentList.Add("wslpath");
            process.StartInfo.ArgumentList.Add("-w");
            process.StartInfo.ArgumentList.Add(path);

            if (!process.Start())
            {
                return null;
            }

            if (!process.WaitForExit(2000))
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Ignore best-effort cleanup failures.
                }

                return null;
            }

            if (process.ExitCode != 0)
            {
                return null;
            }

            var output = process.StandardOutput.ReadToEnd().Trim();
            return string.IsNullOrWhiteSpace(output) ? null : output;
        }
        catch
        {
            return null;
        }
    }
}
