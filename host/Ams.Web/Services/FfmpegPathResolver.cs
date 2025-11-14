using System.Runtime.InteropServices;

namespace Ams.Web.Services;

internal static class FfmpegPathResolver
{
    private const string EnvVar = "AMS_FFMPEG_EXE";

    public static string Resolve(string? configuredPath)
    {
        /*
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            var candidate = Path.GetFullPath(Environment.ExpandEnvironmentVariables(configuredPath));
            if (!File.Exists(candidate))
            {
                throw new FileNotFoundException("Configured ffmpeg executable was not found.", candidate);
            }

            return candidate;
        }

        */
        var fromEnv = Environment.GetEnvironmentVariable(EnvVar);
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            var candidate = Path.GetFullPath(Environment.ExpandEnvironmentVariables(fromEnv));
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        var exeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg";
        var baseDir = AppContext.BaseDirectory;
        for (int i = 0; i < 8 && baseDir is not null; i++)
        {
            var candidate = Path.Combine(baseDir, "host", "Ams.Core", "ExtTools", "ffmpeg", "bin", exeName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            candidate = Path.Combine(baseDir, "host", "ExtTools", "ffmpeg", "bin", exeName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            candidate = Path.Combine(baseDir, "ExtTools", "ffmpeg", "bin", exeName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            baseDir = Directory.GetParent(baseDir)?.FullName;
        }

        throw new FileNotFoundException("Unable to locate ffmpeg executable. Set AMS_FFMPEG_EXE or place ffmpeg under host/ExtTools/ffmpeg/bin.");
    }
}
