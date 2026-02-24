using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using Ams.Core.Application.Mfa.Models;
using Ams.Core.Artifacts.Alignment.Mfa;
using Ams.Core.Runtime.Book;

namespace Ams.Core.Application.Mfa;

public sealed class MfaPronunciationProvider : IPronunciationProvider
{
    private readonly MfaService _mfaService;
    private readonly string _g2pModel;
    private const int MaxPronunciationsPerLexeme = 32;
    private static int _nextG2pInvocationId;

    public MfaPronunciationProvider(MfaService? mfaService = null, string? g2pModel = null)
    {
        _mfaService = mfaService ?? new MfaService();
        _g2pModel = g2pModel ?? MfaService.DefaultG2pModel;
    }

    public async Task<IReadOnlyDictionary<string, string[]>> GetPronunciationsAsync(IEnumerable<string> words,
        CancellationToken cancellationToken)
    {
        var normalized = words
            .Select(PronunciationHelper.NormalizeForLookup)
            .Where(token => !string.IsNullOrEmpty(token))
            .Select(token => token!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalized.Count == 0)
        {
            return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        }

        var lexiconCache = new PronunciationLexiconCache(_g2pModel);
        var cachedPronunciations = await lexiconCache.GetManyAsync(normalized, cancellationToken).ConfigureAwait(false);
        var missingLexemes = normalized
            .Where(lexeme => !cachedPronunciations.ContainsKey(lexeme))
            .ToList();

        Log.Debug("Phoneme cache hits {HitCount:n0}/{TotalCount:n0} lexemes for model {Model}",
            cachedPronunciations.Count,
            normalized.Count,
            _g2pModel);

        if (missingLexemes.Count == 0)
        {
            return cachedPronunciations;
        }

        var lexemeComponents = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in missingLexemes)
        {
            if (!lexemeComponents.ContainsKey(entry))
            {
                lexemeComponents[entry] = PronunciationHelper.SplitLexemeIntoWords(entry);
            }
        }

        var atomicWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var components in lexemeComponents.Values)
        {
            foreach (var component in components)
            {
                if (!string.IsNullOrWhiteSpace(component))
                {
                    atomicWords.Add(component);
                }
            }
        }

        var tempRoot = Directory.CreateTempSubdirectory("ams-g2p-");
        try
        {
            var oovPath = Path.Combine(tempRoot.FullName, "oov.txt");
            var outputPath = Path.Combine(tempRoot.FullName, "g2p.txt");

            if (atomicWords.Count == 0)
            {
                return cachedPronunciations;
            }

            await File.WriteAllLinesAsync(oovPath, atomicWords.Select(w => w), cancellationToken).ConfigureAwait(false);

            var context = new MfaChapterContext
            {
                CorpusDirectory = string.Empty,
                OutputDirectory = string.Empty,
                WorkingDirectory = tempRoot.FullName,
                G2pModel = _g2pModel,
                OovListPath = oovPath,
                G2pOutputPath = outputPath
            };

            var (result, invocationTag) = await RunG2pWithProgressAsync(
                    context,
                    outputPath,
                    atomicWords.Count,
                    cancellationToken)
                .ConfigureAwait(false);
            if (result.ExitCode != 0)
            {
                Log.Debug("{Invocation} MFA g2p exited with code {ExitCode}; command={Command}",
                    invocationTag,
                    result.ExitCode,
                    result.Command);
                foreach (var line in result.StdErr.Take(25))
                {
                    Log.Debug("{Invocation} MFA g2p stderr: {Line}", invocationTag, line);
                }

                if (result.StdErr.Count == 0)
                {
                    foreach (var line in result.StdOut.Take(25))
                    {
                        Log.Debug("{Invocation} MFA g2p stdout: {Line}", invocationTag, line);
                    }
                }

                return cachedPronunciations;
            }

            if (!File.Exists(outputPath))
            {
                Log.Debug("{Invocation} MFA g2p output not found at {Path}; falling back to text-only index",
                    invocationTag,
                    outputPath);
                return cachedPronunciations;
            }

            var wordPronunciations = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            int alternateCount = 0;
            foreach (var line in await File.ReadAllLinesAsync(outputPath, cancellationToken).ConfigureAwait(false))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    continue;
                }

                var word = NormalizeVariantKey(parts[0]);
                if (string.IsNullOrEmpty(word))
                {
                    continue;
                }

                var phonemes = string.Join(' ', parts.Skip(1));
                if (!wordPronunciations.TryGetValue(word, out var list))
                {
                    list = new List<string>();
                    wordPronunciations[word] = list;
                }

                if (!list.Contains(phonemes, StringComparer.OrdinalIgnoreCase))
                {
                    list.Add(phonemes);
                }
                else
                {
                    alternateCount++;
                }
            }

            Log.Debug(
                "{Invocation} MFA g2p parsed {UniqueCount} atomic pronunciations ({AlternateCount} duplicate variants ignored) from {OutputPath}",
                invocationTag,
                wordPronunciations.Count,
                alternateCount,
                outputPath);

            if (wordPronunciations.Count == 0)
            {
                var preview = await File.ReadAllTextAsync(outputPath, cancellationToken).ConfigureAwait(false);
                Log.Debug("{Invocation} MFA g2p output appeared empty; first 200 characters: {Preview}",
                    invocationTag,
                    preview.Length > 200 ? preview[..200] : preview);
            }

            var generatedPronunciations = ComposeLexemePronunciations(lexemeComponents, wordPronunciations);
            var cachedUpdates = generatedPronunciations
                .Where(entry => entry.Value is { Length: > 0 })
                .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.OrdinalIgnoreCase);

            if (cachedUpdates.Count > 0)
            {
                var changed = await lexiconCache.MergeAsync(cachedUpdates, cancellationToken).ConfigureAwait(false);
                if (changed > 0)
                {
                    Log.Debug("Phoneme cache updated with {Changed:n0} lexemes for model {Model}", changed, _g2pModel);
                }
            }

            return MergePronunciationMaps(cachedPronunciations, generatedPronunciations);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Debug("Failed to generate pronunciations via MFA; falling back to text-only index ({Message})",
                ex.Message);
            return cachedPronunciations;
        }
        finally
        {
            try
            {
                tempRoot.Delete(recursive: true);
            }
            catch
            {
                // best effort cleanup
            }
        }
    }

    private static readonly Regex VariantSuffixPattern = new(@"^(?<base>.+?)\(\d+\)$", RegexOptions.Compiled);

    private async Task<(MfaCommandResult Result, string InvocationTag)> RunG2pWithProgressAsync(
        MfaChapterContext context,
        string outputPath,
        int requestedWordCount,
        CancellationToken cancellationToken)
    {
        var invocationId = Interlocked.Increment(ref _nextG2pInvocationId);
        var invocationTag = BuildInvocationTag(invocationId);

        var g2pTask = _mfaService.GeneratePronunciationsAsync(context, cancellationToken);
        var stopwatch = Stopwatch.StartNew();
        var interval = TimeSpan.FromSeconds(5);
        var waitingLogInterval = TimeSpan.FromSeconds(30);
        long lastSize = -1;
        TimeSpan lastWaitingLogElapsed = TimeSpan.MinValue;
        TimeSpan lastNoGrowthLogElapsed = TimeSpan.MinValue;

        Log.Debug("{Invocation} MFA g2p start; requested lexemes={Requested:n0}; output={OutputPath}",
            invocationTag,
            requestedWordCount,
            outputPath);

        while (true)
        {
            var completed = await Task.WhenAny(g2pTask, Task.Delay(interval, cancellationToken)).ConfigureAwait(false);
            if (completed == g2pTask)
            {
                break;
            }

            var (exists, sizeBytes) = GetFileState(outputPath);
            var elapsed = stopwatch.Elapsed;
            if (exists)
            {
                var delta = lastSize >= 0 ? sizeBytes - lastSize : 0;
                if (lastSize < 0 || delta != 0)
                {
                    Log.Debug(
                        "{Invocation} MFA g2p in progress ({Elapsed}); requested lexemes={Requested:n0}; output={OutputBytes:n0} bytes ({DeltaSign}{DeltaBytes:n0} bytes since last)",
                        invocationTag,
                        FormatElapsed(elapsed),
                        requestedWordCount,
                        sizeBytes,
                        delta >= 0 ? "+" : "-",
                        Math.Abs(delta));
                    lastNoGrowthLogElapsed = elapsed;
                }
                else if (lastNoGrowthLogElapsed == TimeSpan.MinValue ||
                         elapsed - lastNoGrowthLogElapsed >= waitingLogInterval)
                {
                    Log.Debug(
                        "{Invocation} MFA g2p in progress ({Elapsed}); requested lexemes={Requested:n0}; output unchanged at {OutputBytes:n0} bytes",
                        invocationTag,
                        FormatElapsed(elapsed),
                        requestedWordCount,
                        sizeBytes);
                    lastNoGrowthLogElapsed = elapsed;
                }
            }
            else if (lastWaitingLogElapsed == TimeSpan.MinValue ||
                     elapsed - lastWaitingLogElapsed >= waitingLogInterval)
            {
                Log.Debug(
                    "{Invocation} MFA g2p in progress ({Elapsed}); requested lexemes={Requested:n0}; waiting for output file",
                    invocationTag,
                    FormatElapsed(elapsed),
                    requestedWordCount);
                lastWaitingLogElapsed = elapsed;
            }

            lastSize = sizeBytes;
        }

        var result = await g2pTask.ConfigureAwait(false);
        var (outputExists, outputSizeBytes) = GetFileState(outputPath);
        Log.Debug(
            "{Invocation} MFA g2p completed in {Elapsed}; output file {State} ({Bytes:n0} bytes)",
            invocationTag,
            FormatElapsed(stopwatch.Elapsed),
            outputExists ? "present" : "missing",
            outputSizeBytes);

        return (result, invocationTag);
    }

    private static (bool Exists, long SizeBytes) GetFileState(string path)
    {
        try
        {
            var info = new FileInfo(path);
            info.Refresh();
            return info.Exists ? (true, info.Length) : (false, 0L);
        }
        catch
        {
            return (false, 0L);
        }
    }

    private static string FormatElapsed(TimeSpan elapsed)
    {
        if (elapsed.TotalHours >= 1)
        {
            return elapsed.ToString(@"hh\:mm\:ss");
        }

        return elapsed.ToString(@"mm\:ss");
    }

    private static string BuildInvocationTag(int invocationId)
    {
        var scopeLabel = MfaInvocationContext.Label;
        if (string.IsNullOrWhiteSpace(scopeLabel))
        {
            return $"[g2p#{invocationId}]";
        }

        return $"[g2p#{invocationId}|{scopeLabel}]";
    }

    private static string NormalizeVariantKey(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        raw = raw.Trim();
        var match = VariantSuffixPattern.Match(raw);
        if (match.Success)
        {
            raw = match.Groups["base"].Value;
        }

        var normalized = PronunciationHelper.NormalizeForLookup(raw);
        return normalized ?? string.Empty;
    }

    private static IReadOnlyDictionary<string, string[]> ComposeLexemePronunciations(
        IReadOnlyDictionary<string, IReadOnlyList<string>> lexemeComponents,
        IReadOnlyDictionary<string, List<string>> wordPronunciations)
    {
        var result = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in lexemeComponents)
        {
            var lexeme = kvp.Key;
            var components = kvp.Value;

            if (components.Count == 0)
            {
                result[lexeme] = Array.Empty<string>();
                continue;
            }

            var combinations = new List<string> { string.Empty };

            foreach (var component in components)
            {
                if (!wordPronunciations.TryGetValue(component, out var variants) || variants.Count == 0)
                {
                    variants = new List<string> { component };
                }

                combinations = ExpandCombinations(combinations, variants, MaxPronunciationsPerLexeme);
            }

            result[lexeme] = combinations
                .Select(c => c.Trim())
                .Where(s => s.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(MaxPronunciationsPerLexeme)
                .ToArray();
        }

        return result;
    }

    private static IReadOnlyDictionary<string, string[]> MergePronunciationMaps(
        IReadOnlyDictionary<string, string[]> first,
        IReadOnlyDictionary<string, string[]> second)
    {
        var merged = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var (lexeme, variants) in first)
        {
            if (variants is { Length: > 0 })
            {
                merged[lexeme] = variants;
            }
        }

        foreach (var (lexeme, variants) in second)
        {
            if (variants is { Length: > 0 })
            {
                merged[lexeme] = variants;
            }
        }

        return merged;
    }

    private static List<string> ExpandCombinations(List<string> basePronunciations, List<string> variants, int maxCount)
    {
        if (maxCount <= 0)
        {
            return new List<string>();
        }

        var capacity = basePronunciations.Count * Math.Max(variants.Count, 1);
        var expanded = new List<string>(Math.Min(maxCount, capacity));
        foreach (var prefix in basePronunciations)
        {
            foreach (var variant in variants)
            {
                var builder = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    builder.Append(prefix.TrimEnd());
                    builder.Append(' ');
                }

                builder.Append(variant.Trim());
                expanded.Add(builder.ToString());

                if (expanded.Count >= maxCount)
                {
                    return expanded;
                }
            }
        }

        return expanded;
    }
}
