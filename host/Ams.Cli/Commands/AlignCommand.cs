using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Artifacts;
using Ams.Core.Artifacts.Alignment;
using Ams.Core.Common;
using Ams.Core.Processors.Diffing;
using Ams.Core.Services.Alignment;
using Ams.Cli.Services;
using Ams.Cli.Utilities;

namespace Ams.Cli.Commands;

public static class AlignCommand
{
    private static readonly JsonSerializerOptions JsonWriteOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static Command Create()
    {
        var align = new Command("align", "Alignment utilities");
        align.AddCommand(CreateAnchors());
        align.AddCommand(CreateTranscriptIndex());
        align.AddCommand(CreateHydrateTx());
        return align;
    }

    private static Command CreateAnchors()
    {
        var cmd = new Command("anchors", "Compute n-gram anchors between BookIndex and ASR");

        var indexOption = new Option<FileInfo?>("--index", "Path to BookIndex JSON");
        indexOption.AddAlias("-i");
        var asrOption = new Option<FileInfo?>("--asr-json", "Path to ASR JSON");
        asrOption.AddAlias("-j");
        var outOption = new Option<FileInfo?>("--out", "Output anchors JSON (defaults to <chapter>.align.anchors.json)");
        outOption.AddAlias("-o");

        var detectSectionOption = new Option<bool>("--detect-section", () => true, "Detect section from ASR prefix and restrict window");
        var ngramOption = new Option<int>("--ngram", () => 3, "Anchor n-gram size");
        var targetPerTokensOption = new Option<int>("--target-per-tokens", () => 50, "Approx. 1 anchor per N book tokens");
        var minSeparationOption = new Option<int>("--min-separation", () => 100, "Min token separation when duplicates allowed during relaxation");
        var crossSentencesOption = new Option<bool>("--cross-sentences", () => false, "Allow anchors to cross sentence boundaries");
        var domainStopwordsOption = new Option<bool>("--domain-stopwords", () => true, "Use English/domain stopwords for anchors");
        var asrPrefixTokensOption = new Option<int>("--asr-prefix", () => 8, "ASR tokens to consider for section detection");
        var emitWindowsOption = new Option<bool>("--emit-windows", () => true, "Also emit search windows between anchors");

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

        cmd.SetHandler(async context =>
        {
            var indexFile = CommandInputResolver.ResolveBookIndex(context.ParseResult.GetValueForOption(indexOption));
            var asrFile = CommandInputResolver.ResolveChapterArtifact(context.ParseResult.GetValueForOption(asrOption), "asr.json");
            var outFile = CommandInputResolver.ResolveChapterArtifact(context.ParseResult.GetValueForOption(outOption), "align.anchors.json", mustExist: false);

            var options = new AnchorComputationOptions
            {
                DetectSection = context.ParseResult.GetValueForOption(detectSectionOption),
                NGram = context.ParseResult.GetValueForOption(ngramOption),
                TargetPerTokens = context.ParseResult.GetValueForOption(targetPerTokensOption),
                MinSeparation = context.ParseResult.GetValueForOption(minSeparationOption),
                AllowBoundaryCross = context.ParseResult.GetValueForOption(crossSentencesOption),
                UseDomainStopwords = context.ParseResult.GetValueForOption(domainStopwordsOption),
                AsrPrefixTokens = context.ParseResult.GetValueForOption(asrPrefixTokensOption),
                EmitWindows = context.ParseResult.GetValueForOption(emitWindowsOption)
            };

            try
            {
                await RunAnchorsAsync(indexFile, asrFile, outFile, options, context.GetCancellationToken()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "align anchors command failed");
                Environment.Exit(1);
            }
        });

        return cmd;
    }

    private static Command CreateTranscriptIndex()
    {
        var cmd = new Command("tx", "Validate & compare Book vs ASR using anchors; emit TranscriptIndex (*.tx.json)");

        var indexOption = new Option<FileInfo?>("--index", "Path to BookIndex JSON");
        indexOption.AddAlias("-i");
        var asrOption = new Option<FileInfo?>("--asr-json", "Path to ASR JSON");
        asrOption.AddAlias("-j");
        var audioOption = new Option<FileInfo?>("--audio", "Path to audio file for reference");
        audioOption.AddAlias("-a");
        var outOption = new Option<FileInfo?>("--out", "Output TranscriptIndex JSON (*.tx.json)");
        outOption.AddAlias("-o");

        var detectSectionOption = new Option<bool>("--detect-section", () => true, "Detect section from ASR prefix and restrict window");
        var asrPrefixTokensOption = new Option<int>("--asr-prefix", () => 8, "ASR tokens to consider for section detection");
        var ngramOption = new Option<int>("--ngram", () => 3, "Anchor n-gram size");
        var targetPerTokensOption = new Option<int>("--target-per-tokens", () => 50, "Approx. 1 anchor per N book tokens");
        var minSeparationOption = new Option<int>("--min-separation", () => 100, "Min token separation when duplicates allowed during relaxation");
        var crossSentencesOption = new Option<bool>("--cross-sentences", () => false, "Allow anchors to cross sentence boundaries");
        var domainStopwordsOption = new Option<bool>("--domain-stopwords", () => true, "Use English/domain stopwords for anchors");

        cmd.AddOption(indexOption);
        cmd.AddOption(asrOption);
        cmd.AddOption(audioOption);
        cmd.AddOption(outOption);
        cmd.AddOption(detectSectionOption);
        cmd.AddOption(asrPrefixTokensOption);
        cmd.AddOption(ngramOption);
        cmd.AddOption(targetPerTokensOption);
        cmd.AddOption(minSeparationOption);
        cmd.AddOption(crossSentencesOption);
        cmd.AddOption(domainStopwordsOption);

        cmd.SetHandler(async context =>
        {
            var indexFile = CommandInputResolver.ResolveBookIndex(context.ParseResult.GetValueForOption(indexOption));
            var asrFile = CommandInputResolver.ResolveChapterArtifact(context.ParseResult.GetValueForOption(asrOption), "asr.json");
            var audioFile = CommandInputResolver.RequireAudio(context.ParseResult.GetValueForOption(audioOption));
            var outFile = CommandInputResolver.ResolveChapterArtifact(context.ParseResult.GetValueForOption(outOption), "align.tx.json", mustExist: false);

            var anchorOptions = new AnchorComputationOptions
            {
                DetectSection = context.ParseResult.GetValueForOption(detectSectionOption),
                AsrPrefixTokens = context.ParseResult.GetValueForOption(asrPrefixTokensOption),
                NGram = context.ParseResult.GetValueForOption(ngramOption),
                TargetPerTokens = context.ParseResult.GetValueForOption(targetPerTokensOption),
                MinSeparation = context.ParseResult.GetValueForOption(minSeparationOption),
                AllowBoundaryCross = context.ParseResult.GetValueForOption(crossSentencesOption),
                UseDomainStopwords = context.ParseResult.GetValueForOption(domainStopwordsOption)
            };

            try
            {
                await RunTranscriptIndexAsync(indexFile, asrFile, audioFile, outFile, anchorOptions, context.GetCancellationToken()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "align tx command failed");
                Environment.Exit(1);
            }
        });

        return cmd;
    }

    private static Command CreateHydrateTx()
    {
        var cmd = new Command("hydrate", "Hydrate a TranscriptIndex with token values from BookIndex and ASR (for debugging)");

        var indexOption = new Option<FileInfo?>("--index", "Path to BookIndex JSON");
        indexOption.AddAlias("-i");
        var asrOption = new Option<FileInfo?>("--asr-json", "Path to ASR JSON");
        asrOption.AddAlias("-j");
        var txOption = new Option<FileInfo?>("--tx-json", "Path to TranscriptIndex JSON");
        txOption.AddAlias("-t");
        var outOption = new Option<FileInfo?>("--out", "Output hydrated JSON file");
        outOption.AddAlias("-o");

        cmd.AddOption(indexOption);
        cmd.AddOption(asrOption);
        cmd.AddOption(txOption);
        cmd.AddOption(outOption);

        cmd.SetHandler(async context =>
        {
            var indexFile = CommandInputResolver.ResolveBookIndex(context.ParseResult.GetValueForOption(indexOption));
            var asrFile = CommandInputResolver.ResolveChapterArtifact(context.ParseResult.GetValueForOption(asrOption), "asr.json");
            var txFile = CommandInputResolver.ResolveChapterArtifact(context.ParseResult.GetValueForOption(txOption), "align.tx.json");
            var outFile = CommandInputResolver.ResolveChapterArtifact(context.ParseResult.GetValueForOption(outOption), "align.hydrate.json", mustExist: false);

            try
            {
                await RunHydrateTxAsync(indexFile, asrFile, txFile, outFile, context.GetCancellationToken()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "align hydrate command failed");
                Environment.Exit(1);
            }
        });

        return cmd;
    }

    internal static async Task RunAnchorsAsync(
        FileInfo indexFile,
        FileInfo asrFile,
        FileInfo outFile,
        AnchorComputationOptions options,
        CancellationToken cancellationToken = default)
    {
        using var handle = ChapterContextFactory.Create(indexFile, asrFile: asrFile);
        var service = CreateAlignmentService();
        var anchors = await service.ComputeAnchorsAsync(handle.Chapter, options, cancellationToken).ConfigureAwait(false);
        await WriteJsonAsync(outFile, anchors, cancellationToken).ConfigureAwait(false);
        handle.Save();
    }

    internal static async Task RunTranscriptIndexAsync(
        FileInfo indexFile,
        FileInfo asrFile,
        FileInfo audioFile,
        FileInfo outFile,
        AnchorComputationOptions anchorOptions,
        CancellationToken cancellationToken = default)
    {
        using var handle = ChapterContextFactory.Create(
            bookIndexFile: indexFile,
            asrFile: asrFile,
            audioFile: audioFile);

        var service = CreateAlignmentService();
        var transcript = await service.BuildTranscriptIndexAsync(handle.Chapter, new TranscriptBuildOptions
        {
            AudioPath = audioFile.FullName,
            ScriptPath = asrFile.FullName,
            BookIndexPath = indexFile.FullName,
            AnchorOptions = anchorOptions
        }, cancellationToken).ConfigureAwait(false);

        await WriteJsonAsync(outFile, transcript, cancellationToken).ConfigureAwait(false);
        handle.Save();
    }

    internal static async Task RunHydrateTxAsync(
        FileInfo indexFile,
        FileInfo asrFile,
        FileInfo txFile,
        FileInfo outFile,
        CancellationToken cancellationToken = default)
    {
        using var handle = ChapterContextFactory.Create(
            bookIndexFile: indexFile,
            asrFile: asrFile,
            transcriptFile: txFile);

        var service = CreateAlignmentService();
        var hydrated = await service.HydrateTranscriptAsync(handle.Chapter, null, cancellationToken).ConfigureAwait(false);
        await WriteJsonAsync(outFile, hydrated, cancellationToken).ConfigureAwait(false);
        handle.Save();
    }

    private static IAlignmentService CreateAlignmentService()
        => new AlignmentService(new MfaPronunciationProvider());

    private static async Task WriteJsonAsync<T>(FileInfo target, T payload, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(target.Directory?.FullName ?? Directory.GetCurrentDirectory());
        var json = JsonSerializer.Serialize(payload, JsonWriteOptions);
        await File.WriteAllTextAsync(target.FullName, json, cancellationToken).ConfigureAwait(false);
        Log.Debug("Wrote {Path}", target.FullName);
    }
}
