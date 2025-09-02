using System.CommandLine;
using System.Text.Json;
using Ams.Align.Anchors;
using Ams.Core;
using Ams.Core.Validation;

namespace Ams.Cli.Commands;

public static class AlignCommand
{
    public static Command Create()
    {
        var align = new Command("align", "Alignment utilities");
        align.AddCommand(CreateAnchors());
        return align;
    }

    private static Command CreateAnchors()
    {
        var cmd = new Command("anchors", "Compute n-gram anchors between BookIndex and ASR");

        var indexOption = new Option<FileInfo>("--index", "Path to BookIndex JSON") { IsRequired = true };
        indexOption.AddAlias("-i");
        var asrOption = new Option<FileInfo>("--asr-json", "Path to ASR JSON") { IsRequired = true };
        asrOption.AddAlias("-j");
        var outOption = new Option<FileInfo?>("--out", () => null, "Output JSON file (default: stdout)");

        var detectSectionOption = new Option<bool>("--detect-section", () => true, "Detect section from ASR prefix and restrict window");
        var ngramOption = new Option<int>("--ngram", () => 3, "Anchor n-gram size");
        var targetPerTokensOption = new Option<int>("--target-per-tokens", () => 50, "Approx. 1 anchor per N book tokens");
        var minSeparationOption = new Option<int>("--min-separation", () => 100, "Min token separation when duplicates allowed during relaxation");
        var crossSentencesOption = new Option<bool>("--cross-sentences", () => false, "Allow anchors to cross sentence boundaries");
        var domainStopwordsOption = new Option<bool>("--domain-stopwords", () => true, "Use English/domain stopwords");

        var asrPrefixTokensOption = new Option<int>("--asr-prefix", () => 8, "ASR tokens to consider for section detection");
        var emitWindowsOption = new Option<bool>("--emit-windows", () => false, "Also emit search windows between anchors");

        cmd.AddOption(indexOption);
        cmd.AddOption(asrOption);
        cmd.AddOption(outOption);
        cmd.AddOption(detectSectionOption);
        cmd.AddOption(ngramOption);
        cmd.AddOption(targetPerTokensOption);
        cmd.AddOption(minSeparationOption);
        cmd.AddOption(crossSentencesOption);
        cmd.AddOption(domainStopwordsOption);
        cmd.AddOption(asrPrefixTokensOption);
        cmd.AddOption(emitWindowsOption);

        cmd.SetHandler(async (context) =>
        {
            var indexFile = context.ParseResult.GetValueForOption(indexOption)!;
            var asrFile = context.ParseResult.GetValueForOption(asrOption)!;
            var outFile = context.ParseResult.GetValueForOption(outOption);
            var detectSection = context.ParseResult.GetValueForOption(detectSectionOption);
            var ngram = context.ParseResult.GetValueForOption(ngramOption);
            var targetPerTokens = context.ParseResult.GetValueForOption(targetPerTokensOption);
            var minSeparation = context.ParseResult.GetValueForOption(minSeparationOption);
            var crossSentences = context.ParseResult.GetValueForOption(crossSentencesOption);
            var domainStopwords = context.ParseResult.GetValueForOption(domainStopwordsOption);
            var asrPrefix = context.ParseResult.GetValueForOption(asrPrefixTokensOption);
            var emitWindows = context.ParseResult.GetValueForOption(emitWindowsOption);

            try
            {
                await RunAnchorsAsync(indexFile, asrFile, outFile, detectSection, ngram, targetPerTokens, minSeparation, crossSentences, domainStopwords, asrPrefix, emitWindows);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        });

        return cmd;
    }

    private static async Task RunAnchorsAsync(
        FileInfo indexFile,
        FileInfo asrFile,
        FileInfo? outFile,
        bool detectSection,
        int ngram,
        int targetPerTokens,
        int minSeparation,
        bool crossSentences,
        bool domainStopwords,
        int asrPrefixTokens,
        bool emitWindows)
    {
        if (!indexFile.Exists) throw new FileNotFoundException($"Index file not found: {indexFile.FullName}");
        if (!asrFile.Exists) throw new FileNotFoundException($"ASR JSON file not found: {asrFile.FullName}");

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var indexJson = await File.ReadAllTextAsync(indexFile.FullName);
        var book = JsonSerializer.Deserialize<BookIndex>(indexJson, jsonOptions) ?? throw new InvalidOperationException("Failed to parse BookIndex JSON");

        var asrJson = await File.ReadAllTextAsync(asrFile.FullName);
        var asr = JsonSerializer.Deserialize<AsrResponse>(asrJson, jsonOptions) ?? throw new InvalidOperationException("Failed to parse ASR JSON");

        // Build policy
        var stop = domainStopwords ? StopwordSets.EnglishPlusDomain : new HashSet<string>(StringComparer.Ordinal);
        var policy = new AnchorPolicy(
            NGram: ngram,
            TargetPerTokens: targetPerTokens,
            AllowDuplicates: false,
            MinSeparation: minSeparation,
            Stopwords: stop,
            DisallowBoundaryCross: !crossSentences
        );

        var secOpts = new SectionDetectOptions(Detect: detectSection, AsrPrefixTokens: asrPrefixTokens);
        var pipe = AnchorPipeline.ComputeAnchors(book, asr, policy, secOpts, includeWindows: emitWindows);

        var anchorsOut = pipe.Anchors.Select(a => new
        {
            bp = a.Bp,
            bpWordIndex = pipe.BookFilteredToOriginalWord[a.Bp],
            ap = a.Ap
        }).ToList();

        var payload = new
        {
            sectionDetected = pipe.SectionDetected,
            section = pipe.Section == null ? null : new { pipe.Section.Id, pipe.Section.Title, pipe.Section.Level, pipe.Section.Kind, pipe.Section.StartWord, pipe.Section.EndWord },
            policy = new { ngram, targetPerTokens, minSeparation, disallowBoundaryCross = !crossSentences, stopwords = domainStopwords ? "domain" : "none" },
            tokens = new { bookCount = pipe.BookTokenCount, bookFilteredCount = pipe.BookFilteredCount, asrCount = pipe.AsrTokenCount, asrFilteredCount = pipe.AsrFilteredCount },
            window = new { bookStart = pipe.BookWindowFiltered.bStart, bookEnd = pipe.BookWindowFiltered.bEnd },
            anchors = anchorsOut,
            windows = pipe.Windows?.Select(w => new { bLo = w.bLo, bHi = w.bHi, aLo = w.aLo, aHi = w.aHi })
        };

        var outputJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        if (outFile == null)
        {
            Console.WriteLine(outputJson);
        }
        else
        {
            await File.WriteAllTextAsync(outFile.FullName, outputJson);
            Console.WriteLine($"Anchors written to: {outFile.FullName}");
        }
    }
}
