using System.IO;

namespace Ams.Core.Pipeline;

/// <summary>
/// Minimal manifest shape for pipeline stages while the broader orchestrator evolves.
/// </summary>
public sealed record ManifestV2(
    string ChapterId,
    string WorkDirectory,
    string AudioPath,
    string TranscriptIndexPath)
{
    public string ResolveStageDirectory(string stageName)
    {
        if (string.IsNullOrWhiteSpace(stageName)) throw new ArgumentException("Stage name required", nameof(stageName));
        var dir = Path.Combine(WorkDirectory, stageName);
        Directory.CreateDirectory(dir);
        return dir;
    }
}

