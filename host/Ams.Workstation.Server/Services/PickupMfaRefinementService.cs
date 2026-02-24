using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ams.Core.Application.Mfa;
using Ams.Core.Application.Mfa.Models;
using Ams.Core.Application.Processes;
using Ams.Core.Artifacts.Hydrate;
using Ams.Core.Common;
using Ams.Core.Processors.Alignment.Mfa;
using Ams.Core.Runtime.Book;
using Ams.Workstation.Server.Models;

namespace Ams.Workstation.Server.Services;

/// <summary>
/// Runs MFA forced alignment on pickup audio to replace rough ASR token
/// boundaries with phoneme-accurate sentence timings.  Takes ASR-matched
/// <see cref="PickupMatch"/> records and refines their PickupStartSec /
/// PickupEndSec using TextGrid word intervals mapped back via
/// Needleman-Wunsch alignment (same strategy as <see cref="MfaTimingMerger"/>).
/// Falls back to original ASR timings on any MFA failure.
/// </summary>
public class PickupMfaRefinementService
{
    private static readonly JsonSerializerOptions CacheJsonOptions = new()
    {
        WriteIndented = false
    };

    private readonly BlazorWorkspace _workspace;

    public PickupMfaRefinementService(BlazorWorkspace workspace)
    {
        _workspace = workspace;
    }

    /// <summary>
    /// Refines ASR-based pickup match timings using MFA forced alignment.
    /// </summary>
    /// <param name="pickupFilePath">Path to the pickup WAV file.</param>
    /// <param name="asrMatches">ASR-matched pickup records (with approximate timings).</param>
    /// <param name="flaggedSentences">The flagged sentences that were matched against.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Refined pickup matches with MFA-accurate timings, or originals on failure.</returns>
    public async Task<IReadOnlyList<PickupMatch>> RefineWithMfaAsync(
        string pickupFilePath,
        IReadOnlyList<PickupMatch> asrMatches,
        IReadOnlyList<HydratedSentence> flaggedSentences,
        CancellationToken ct)
    {
        if (asrMatches.Count == 0)
            return asrMatches;

        string? corpusDir = null;
        try
        {
            var workDir = _workspace.WorkingDirectory
                ?? throw new InvalidOperationException("No working directory set.");

            // Order matches by pickup position for consistent processing
            var orderedMatches = asrMatches.OrderBy(m => m.PickupStartSec).ToList();

            // Build a lookup from sentence id to HydratedSentence
            var sentenceLookup = flaggedSentences.ToDictionary(s => s.Id);

            // Compute cache key incorporating both audio identity and matched sentence context
            var cacheHash = ComputeMfaCacheKey(pickupFilePath, orderedMatches, sentenceLookup);
            var cacheDir = Path.Combine(workDir, ".polish", "pickups");
            var cachePath = Path.Combine(cacheDir, $"{cacheHash}.mfa.json");

            // 1. Check MFA cache
            var cached = TryReadMfaCache(cachePath);
            if (cached != null)
            {
                Log.Debug("MFA pickup cache hit ({Hash})", cacheHash);
                return cached;
            }

            // 2. Build temp corpus directory
            corpusDir = Path.Combine(workDir, ".polish", "pickups", "mfa", cacheHash);
            Directory.CreateDirectory(corpusDir);

            var stagedWavPath = Path.Combine(corpusDir, "pickup.wav");
            File.Copy(pickupFilePath, stagedWavPath, overwrite: true);

            // Build .lab file from concatenated BookText of all matched sentences
            var labPath = Path.Combine(corpusDir, "pickup.lab");
            var labContent = BuildLabContent(orderedMatches, sentenceLookup);
            if (string.IsNullOrWhiteSpace(labContent))
            {
                Log.Debug("MFA pickup refinement: no usable lab content, returning ASR timings");
                return asrMatches;
            }
            await File.WriteAllTextAsync(labPath, labContent, Encoding.UTF8, ct).ConfigureAwait(false);

            // 3. Ensure MFA supervisor is warm
            await MfaProcessSupervisor.EnsureReadyAsync(ct).ConfigureAwait(false);

            // 4. Build MfaChapterContext
            var outputDir = Path.Combine(corpusDir, "output");
            Directory.CreateDirectory(outputDir);

            var context = new MfaChapterContext
            {
                CorpusDirectory = corpusDir,
                OutputDirectory = outputDir,
                WorkingDirectory = corpusDir,
                DictionaryModel = MfaService.DefaultDictionaryModel,
                AcousticModel = MfaService.DefaultAcousticModel,
                G2pModel = MfaService.DefaultG2pModel,
                Beam = 80,
                RetryBeam = 200,
                SingleSpeaker = true,
                CleanOutput = true
            };

            // 5. Run MFA validate + G2P + align
            var service = new MfaService(useDedicatedProcess: false);

            // Validate to discover OOVs
            var validateResult = await service.ValidateAsync(context, ct).ConfigureAwait(false);
            LogMfaResult("mfa validate (pickup)", validateResult);

            // Check for OOVs and handle with G2P if found
            var mfaRoot = corpusDir;
            var oovPath = FindOovListFile(mfaRoot);
            if (oovPath != null)
            {
                var g2pOutputPath = Path.Combine(mfaRoot, "pickup.g2p.txt");
                var customDictPath = Path.Combine(mfaRoot, "pickup.dictionary.zip");

                var g2pContext = context with
                {
                    OovListPath = oovPath,
                    G2pOutputPath = g2pOutputPath,
                    CustomDictionaryPath = customDictPath
                };

                var g2pResult = await service.GeneratePronunciationsAsync(g2pContext, ct)
                    .ConfigureAwait(false);
                LogMfaResult("mfa g2p (pickup)", g2pResult);

                if (g2pResult.ExitCode == 0 && File.Exists(g2pOutputPath) && new FileInfo(g2pOutputPath).Length > 0)
                {
                    var addWordsResult = await service.AddWordsAsync(g2pContext, ct).ConfigureAwait(false);
                    LogMfaResult("mfa add_words (pickup)", addWordsResult);

                    if (addWordsResult.ExitCode == 0 && File.Exists(customDictPath))
                    {
                        context = context with { CustomDictionaryPath = customDictPath };
                    }
                }
            }

            // Run alignment
            var alignResult = await service.AlignAsync(context, ct).ConfigureAwait(false);
            if (alignResult.ExitCode != 0)
            {
                Log.Warn("MFA align failed for pickup (exit {Code}), using ASR timings", alignResult.ExitCode);
                return asrMatches;
            }

            // 6. Parse TextGrid
            var textGridPath = FindTextGridFile(outputDir);
            if (textGridPath == null)
            {
                Log.Warn("MFA TextGrid not found after alignment, using ASR timings");
                return asrMatches;
            }

            var intervals = TextGridParser.ParseWordIntervals(textGridPath);

            // 7. Map word intervals to sentences using Needleman-Wunsch alignment
            var refined = MapIntervalsToSentences(intervals, orderedMatches, sentenceLookup);

            // 8. Build refined PickupMatch list (preserve unrefined as fallback)
            var result = BuildRefinedMatches(orderedMatches, refined);

            // 9. Cache results
            WriteMfaCache(cachePath, cacheDir, result);

            Log.Debug("MFA pickup refinement complete: {Refined}/{Total} matches refined",
                refined.Count, orderedMatches.Count);

            return result;
        }
        catch (OperationCanceledException)
        {
            throw; // Preserve caller cancellation semantics
        }
        catch (Exception ex)
        {
            Log.Warn("MFA pickup refinement failed ({Message}), returning original ASR timings", ex.Message);
            return asrMatches;
        }
        finally
        {
            if (corpusDir != null)
                CleanupCorpusDir(corpusDir);
        }
    }

    #region Lab file construction

    /// <summary>
    /// Builds MFA .lab file content from the BookText of all matched sentences,
    /// joined into a single continuous line of pronunciation-normalized words.
    /// </summary>
    private static string BuildLabContent(
        List<PickupMatch> orderedMatches,
        Dictionary<int, HydratedSentence> sentenceLookup)
    {
        var allParts = new List<string>();

        foreach (var match in orderedMatches)
        {
            if (!sentenceLookup.TryGetValue(match.SentenceId, out var sentence))
                continue;

            var parts = PronunciationHelper.ExtractPronunciationParts(sentence.BookText);
            allParts.AddRange(parts);
        }

        return allParts.Count > 0 ? string.Join(' ', allParts) : string.Empty;
    }

    #endregion

    #region Needleman-Wunsch sentence mapping

    /// <summary>
    /// Uses MfaTimingMerger.MergeAndApply with synthetic book indices to map
    /// TextGrid word intervals back to matched sentences.
    /// </summary>
    private static Dictionary<int, (double Start, double End)> MapIntervalsToSentences(
        IReadOnlyList<TextGridInterval> intervals,
        List<PickupMatch> orderedMatches,
        Dictionary<int, HydratedSentence> sentenceLookup)
    {
        // Build flat list of expected words across all matched sentences,
        // tracking which sentence each word belongs to via synthetic bookIdx
        var flatWords = new List<string>();
        var sentenceRanges = new List<(int SentenceId, int StartIdx, int EndIdx)>();
        int currentIdx = 0;

        foreach (var match in orderedMatches)
        {
            if (!sentenceLookup.TryGetValue(match.SentenceId, out var sentence))
                continue;

            var parts = PronunciationHelper.ExtractPronunciationParts(sentence.BookText);
            if (parts.Count == 0) continue;

            int startIdx = currentIdx;
            foreach (var part in parts)
            {
                flatWords.Add(part);
                currentIdx++;
            }
            sentenceRanges.Add((match.SentenceId, startIdx, currentIdx - 1));
        }

        if (flatWords.Count == 0 || intervals.Count == 0)
            return new Dictionary<int, (double, double)>();

        // Create TextGridWord[] from parsed intervals (filter silence/empty entries)
        var textGridWords = intervals
            .Where(iv => !string.IsNullOrWhiteSpace(iv.Text)
                         && !string.Equals(iv.Text, "sp", StringComparison.OrdinalIgnoreCase)
                         && !string.Equals(iv.Text, "sil", StringComparison.OrdinalIgnoreCase)
                         && !string.Equals(iv.Text, "", StringComparison.Ordinal))
            .Select(iv => new TextGridWord(iv.Text, iv.Start, iv.End))
            .ToArray();

        if (textGridWords.Length == 0)
            return new Dictionary<int, (double, double)>();

        // Build getBookToken function from flat word list
        string GetBookToken(int idx) => idx >= 0 && idx < flatWords.Count ? flatWords[idx] : "";

        // Create empty WordTarget array (we don't need per-word timings, only per-sentence)
        var wordTargets = Array.Empty<WordTarget>();

        // Create SentenceTarget[] from sentenceRanges that capture timings
        var sentenceTimings = new Dictionary<int, (double Start, double End)>();
        var sentenceTargets = sentenceRanges.Select(r =>
            new SentenceTarget(
                r.StartIdx,
                r.EndIdx,
                (start, end, _) => sentenceTimings[r.SentenceId] = (start, end)))
            .ToArray();

        // Run Needleman-Wunsch alignment via MfaTimingMerger
        MfaTimingMerger.MergeAndApply(
            textGridWords,
            GetBookToken,
            0,
            flatWords.Count - 1,
            wordTargets,
            sentenceTargets,
            debugLog: msg => Log.Debug("MFA pickup merge: {Message}", msg));

        return sentenceTimings;
    }

    #endregion

    #region Result building

    private static List<PickupMatch> BuildRefinedMatches(
        List<PickupMatch> orderedMatches,
        Dictionary<int, (double Start, double End)> mfaTimings)
    {
        var result = new List<PickupMatch>(orderedMatches.Count);

        foreach (var match in orderedMatches)
        {
            if (mfaTimings.TryGetValue(match.SentenceId, out var timing))
            {
                result.Add(match with
                {
                    PickupStartSec = timing.Start,
                    PickupEndSec = timing.End
                });
            }
            else
            {
                // Keep original ASR timing as fallback
                result.Add(match);
            }
        }

        return result;
    }

    #endregion

    #region MFA Cache

    private static string ComputeMfaCacheKey(
        string pickupFilePath,
        List<PickupMatch> orderedMatches,
        Dictionary<int, HydratedSentence> sentenceLookup)
    {
        var fi = new FileInfo(pickupFilePath);
        var sb = new StringBuilder();
        sb.Append(fi.FullName).Append('|').Append(fi.Length).Append('|').Append(fi.LastWriteTimeUtc.ToString("O"));

        // Include sentence IDs
        sb.Append('|');
        sb.Append(string.Join(":", orderedMatches.Select(m => m.SentenceId)));

        // Include normalized book text for each matched sentence
        foreach (var match in orderedMatches)
        {
            if (sentenceLookup.TryGetValue(match.SentenceId, out var sentence))
            {
                sb.Append('|');
                var parts = PronunciationHelper.ExtractPronunciationParts(sentence.BookText);
                sb.Append(string.Join(' ', parts));
            }
        }

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexString(hashBytes)[..16].ToLowerInvariant();
    }

    private static List<PickupMatch>? TryReadMfaCache(string cachePath)
    {
        if (!File.Exists(cachePath))
            return null;

        try
        {
            var json = File.ReadAllText(cachePath);
            return JsonSerializer.Deserialize<List<PickupMatch>>(json);
        }
        catch
        {
            return null;
        }
    }

    private static void WriteMfaCache(string cachePath, string cacheDir, List<PickupMatch> result)
    {
        try
        {
            Directory.CreateDirectory(cacheDir);
            var json = JsonSerializer.Serialize(result, CacheJsonOptions);
            File.WriteAllText(cachePath, json);
        }
        catch (Exception ex)
        {
            Log.Debug("Failed to write MFA pickup cache: {Message}", ex.Message);
        }
    }

    #endregion

    #region MFA helpers

    private static string? FindTextGridFile(string outputDir)
    {
        var candidates = new[]
        {
            Path.Combine(outputDir, "alignment", "mfa", "pickup.TextGrid"),
            Path.Combine(outputDir, "pickup.TextGrid")
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private static string? FindOovListFile(string directory)
    {
        try
        {
            return Directory.EnumerateFiles(directory, "oovs_found*.txt", SearchOption.AllDirectories)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private static void LogMfaResult(string stage, MfaCommandResult result)
    {
        foreach (var line in result.StdOut)
            Log.Debug("{Stage}> {Line}", stage, line);
        foreach (var line in result.StdErr)
            Log.Debug("{Stage}! {Line}", stage, line);
        Log.Debug("{Stage} exit code: {ExitCode}", stage, result.ExitCode);
    }

    private static void CleanupCorpusDir(string corpusDir)
    {
        try
        {
            if (Directory.Exists(corpusDir))
                Directory.Delete(corpusDir, recursive: true);
        }
        catch (Exception ex)
        {
            Log.Debug("Failed to clean up MFA pickup corpus dir: {Message}", ex.Message);
        }
    }

    #endregion
}
