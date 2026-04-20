using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Deterministic pickup import service for a single stitched session WAV.
/// CRX targets define the required target order; no text-based disambiguation or fallback matching is allowed.
/// </summary>
public class PickupAssetService
{
    private readonly PickupMatchingService _pickupMatching;

    public PickupAssetService(PickupMatchingService pickupMatching, BlazorWorkspace workspace)
    {
        _pickupMatching = pickupMatching;
        _ = workspace;
    }

    public async Task<(IReadOnlyList<PickupAsset> Matched, IReadOnlyList<PickupAsset> Unmatched)> ImportAsync(
        string sourcePath,
        IReadOnlyList<CrxPickupTarget> crxTargets,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentNullException.ThrowIfNull(crxTargets);

        if (Directory.Exists(sourcePath))
        {
            throw new InvalidOperationException(
                "Folder-based pickup import is no longer supported. Provide a single stitched pickup WAV.");
        }

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException($"Source path does not exist: '{sourcePath}'");
        }

        return await ImportFromSessionFileAsync(sourcePath, crxTargets, ct).ConfigureAwait(false);
    }

    public async Task<(IReadOnlyList<PickupAsset> Matched, IReadOnlyList<PickupAsset> Unmatched)> ImportFromSessionFileAsync(
        string sessionFilePath,
        IReadOnlyList<CrxPickupTarget> crxTargets,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionFilePath);
        ArgumentNullException.ThrowIfNull(crxTargets);

        if (!File.Exists(sessionFilePath))
            throw new FileNotFoundException($"Session file does not exist: '{sessionFilePath}'");

        if (crxTargets.Count == 0)
        {
            throw new InvalidOperationException(
                "Pickup import requires CRX targets. No CRX targets were resolved.");
        }

        var fi = new FileInfo(sessionFilePath);

        var matches = await _pickupMatching.MatchPickupCrxAsync(sessionFilePath, crxTargets, ct)
            .ConfigureAwait(false);
        var sortedTargets = crxTargets.OrderBy(t => t.ErrorNumber).ToList();

        if (matches.Count != sortedTargets.Count)
        {
            throw new InvalidOperationException(
                $"Deterministic pickup import produced {matches.Count} matches for {sortedTargets.Count} CRX targets.");
        }

        var assets = new List<PickupAsset>(matches.Count);
        var now = DateTime.UtcNow;

        for (var i = 0; i < matches.Count; i++)
        {
            var match = matches[i];
            var target = sortedTargets[i];

            if (match.ErrorNumber != target.ErrorNumber)
            {
                throw new InvalidOperationException(
                    $"Deterministic pickup import lost CRX ordering at index {i}: expected error #{target.ErrorNumber}, got #{match.ErrorNumber}.");
            }

            assets.Add(new PickupAsset(
                Id: Guid.NewGuid().ToString("N"),
                SourceType: PickupSourceType.SessionSegment,
                SourceFilePath: fi.FullName,
                TrimStartSec: match.PickupStartSec,
                TrimEndSec: match.PickupEndSec,
                TranscribedText: match.RecognizedText,
                Confidence: 1.0,
                MatchedErrorNumber: target.ErrorNumber,
                MatchedSentenceId: target.SentenceId,
                MatchedChapterStem: target.ChapterStem,
                ImportedAtUtc: now));
        }

        return (assets, Array.Empty<PickupAsset>());
    }
}
