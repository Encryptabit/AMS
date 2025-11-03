using System;
using System.Collections.Generic;
using System.IO;
using Ams.Cli.Repl;
using Ams.Core.Common;
using Ams.Core.Prosody;

namespace Ams.Cli.Utilities;

internal static class PausePolicyResolver
{
    public static (PausePolicy Policy, string? SourcePath) Resolve(FileInfo? transcriptFile = null)
    {
        var candidates = EnumerateCandidates(transcriptFile);
        foreach (var path in candidates)
        {
            try
            {
                if (!File.Exists(path))
                {
                    continue;
                }

                var policy = PausePolicyStorage.Load(path);
                return (policy, path);
            }
            catch (Exception ex)
            {
                Log.Warn("Failed to load pause-policy.json from {Path}; trying next candidate. Error={Message}", path, ex.Message);
            }
        }

        return (PausePolicyPresets.House(), null);
    }

    private static IEnumerable<string> EnumerateCandidates(FileInfo? transcriptFile)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ordered = new List<string>();

        if (transcriptFile is not null)
        {
            var dir = transcriptFile.DirectoryName;
            if (!string.IsNullOrWhiteSpace(dir))
            {
                AddCandidate(seen, ordered, Path.Combine(dir, "pause-policy.json"));

                var parent = Directory.GetParent(dir);
                if (parent is not null)
                {
                    AddCandidate(seen, ordered, Path.Combine(parent.FullName, "pause-policy.json"));
                }
            }
        }

        var replWorking = ReplContext.Current?.WorkingDirectory;
        if (!string.IsNullOrWhiteSpace(replWorking))
        {
            AddCandidate(seen, ordered, Path.Combine(replWorking, "pause-policy.json"));
        }

        var cwd = Directory.GetCurrentDirectory();
        AddCandidate(seen, ordered, Path.Combine(cwd, "pause-policy.json"));

        return ordered;
    }

    private static void AddCandidate(HashSet<string> seen, List<string> ordered, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var full = Path.GetFullPath(path);
        if (seen.Add(full))
        {
            ordered.Add(full);
        }
    }
}
