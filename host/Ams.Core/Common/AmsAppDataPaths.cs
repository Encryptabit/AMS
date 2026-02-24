using System;
using System.IO;

namespace Ams.Core.Common;

/// <summary>
/// Resolves AMS app data locations using LocalApplicationData across environments.
/// </summary>
public static class AmsAppDataPaths
{
    public const string AmsFolderName = "AMS";

    /// <summary>
    /// Gets the AMS root path under LocalApplicationData (falls back to user profile when unavailable).
    /// </summary>
    public static string RootPath
    {
        get
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(basePath))
            {
                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                basePath = Path.Combine(userProfile, ".local", "share");
            }

            return Path.Combine(basePath, AmsFolderName);
        }
    }

    /// <summary>
    /// Resolves a path inside the AMS app data root.
    /// </summary>
    public static string Resolve(params string[] segments)
    {
        ArgumentNullException.ThrowIfNull(segments);

        var path = RootPath;
        for (var i = 0; i < segments.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(segments[i])) continue;
            path = Path.Combine(path, segments[i]);
        }

        return path;
    }
}
