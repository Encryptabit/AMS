using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ams.Core.Io;

public static class PathNormalizer
{
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    public static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        // Convert backslashes to forward slashes for consistency
        var normalized = path.Replace('\\', '/');

        // If we're running on Linux/WSL and receive a Windows path, convert to WSL format
        if (IsLinux && IsWindowsPath(normalized))
        {
            return ConvertWindowsToWslPath(normalized);
        }

        // If we're running on Windows and receive a WSL path, convert to Windows format
        if (IsWindows && IsWslPath(normalized))
        {
            return ConvertWslToWindowsPath(normalized);
        }

        return normalized;
    }

    public static string ToWindowsPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        var normalized = path.Replace('\\', '/');

        if (IsWslPath(normalized))
        {
            return ConvertWslToWindowsPath(normalized);
        }

        return normalized.Replace('/', '\\');
    }

    public static string ToWslPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        var normalized = path.Replace('\\', '/');

        if (IsWindowsPath(normalized))
        {
            return ConvertWindowsToWslPath(normalized);
        }

        return normalized;
    }

    private static bool IsWindowsPath(string path)
    {
        // Check for C:/ pattern
        return path.Length >= 3 && path[1] == ':' && path[2] == '/';
    }

    private static bool IsWslPath(string path)
    {
        // Check for /mnt/c/ pattern
        return path.StartsWith("/mnt/") && path.Length >= 6 && path[5] == '/';
    }

    private static string ConvertWindowsToWslPath(string windowsPath)
    {
        // Convert C:/path/to/file to /mnt/c/path/to/file
        if (windowsPath.Length < 3 || windowsPath[1] != ':')
            return windowsPath;

        var driveLetter = char.ToLower(windowsPath[0]);
        var restOfPath = windowsPath.Substring(3); // Skip "C:/"
        
        return $"/mnt/{driveLetter}/{restOfPath}";
    }

    private static string ConvertWslToWindowsPath(string wslPath)
    {
        // Convert /mnt/c/path/to/file to C:/path/to/file
        if (!wslPath.StartsWith("/mnt/") || wslPath.Length < 6)
            return wslPath;

        var driveLetter = char.ToUpper(wslPath[5]);
        var restOfPath = wslPath.Length > 7 ? wslPath.Substring(7) : ""; // Skip "/mnt/c/"
        
        return $"{driveLetter}:/{restOfPath}";
    }

    public static bool PathExists(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        var normalized = NormalizePath(path);
        return File.Exists(normalized) || Directory.Exists(normalized);
    }

    public static void EnsureDirectory(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        var normalized = NormalizePath(path);
        var directory = Path.GetDirectoryName(normalized);
        
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}