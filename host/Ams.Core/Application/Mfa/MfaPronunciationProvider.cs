using System.Text;
using System.Text.RegularExpressions;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Artifacts.Alignment.Mfa;
using Ams.Core.Runtime.Book;

namespace Ams.Core.Application.Mfa;

public sealed class MfaPronunciationProvider : IPronunciationProvider
{
    private readonly IMfaService _mfaService;
    private readonly string _g2pModel;
    private const int MaxPronunciationsPerLexeme = 32;

    public MfaPronunciationProvider(IMfaService? mfaService = null, string? g2pModel = null)
    {
        _mfaService = mfaService ?? new MfaService();
        _g2pModel = g2pModel ?? MfaService.DefaultG2pModel;
    }

    public async Task<IReadOnlyDictionary<string, string[]>> GetPronunciationsAsync(IEnumerable<string> words, CancellationToken cancellationToken)
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

        var lexemeComponents = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in normalized)
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
                return normalized.ToDictionary(lex => lex, lex => Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
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

            var result = await _mfaService.GeneratePronunciationsAsync(context, cancellationToken).ConfigureAwait(false);
            if (result.ExitCode != 0)
            {
                Log.Debug("MFA g2p exited with code {ExitCode}; falling back to text-only index", result.ExitCode);
                return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            }

            if (!File.Exists(outputPath))
            {
                Log.Debug("MFA g2p output not found at {Path}; falling back to text-only index", outputPath);
                return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
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
                "MFA g2p parsed {UniqueCount} atomic pronunciations ({AlternateCount} duplicate variants ignored) from {OutputPath}",
                wordPronunciations.Count,
                alternateCount,
                outputPath);

            if (wordPronunciations.Count == 0)
            {
                var preview = await File.ReadAllTextAsync(outputPath, cancellationToken).ConfigureAwait(false);
                Log.Debug("MFA g2p output appeared empty; first 200 characters: {Preview}", preview.Length > 200 ? preview[..200] : preview);
            }

            return ComposeLexemePronunciations(lexemeComponents, wordPronunciations);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Debug("Failed to generate pronunciations via MFA; falling back to text-only index ({Message})", ex.Message);
            return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
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
