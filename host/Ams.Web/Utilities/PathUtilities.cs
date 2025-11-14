using Microsoft.Extensions.Hosting;

namespace Ams.Web.Utilities;

internal static class PathUtilities
{
    public static string ResolveUserPath(string path, IHostEnvironment environment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var expanded = Environment.ExpandEnvironmentVariables(path);
        expanded = expanded.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), StringComparison.Ordinal);
        if (Path.IsPathRooted(expanded))
        {
            return Path.GetFullPath(expanded);
        }

        return Path.GetFullPath(Path.Combine(environment.ContentRootPath, expanded));
    }
}
