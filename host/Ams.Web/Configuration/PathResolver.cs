using Microsoft.Extensions.Hosting;

namespace Ams.Web.Configuration;

internal static class PathResolver
{
    public static string Resolve(string? path, IHostEnvironment environment, bool allowUnrootedExecutable = false)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException("A valid path must be provided in configuration.");
        }

        var expanded = Environment.ExpandEnvironmentVariables(path);
        expanded = expanded.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), StringComparison.Ordinal);

        if (allowUnrootedExecutable && expanded.IndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) < 0)
        {
            return expanded;
        }

        if (Path.IsPathRooted(expanded))
        {
            return Path.GetFullPath(expanded);
        }

        return Path.GetFullPath(Path.Combine(environment.ContentRootPath, expanded));
    }
}
