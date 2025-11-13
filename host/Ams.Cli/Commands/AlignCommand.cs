using System.CommandLine;
using System.IO;
using Ams.Core.Common;
using Ams.Core.Processors.Alignment.Anchors;
using Ams.Cli.Utilities;

namespace Ams.Cli.Commands;

public static class AlignCommand
{
    public static Command Create(
        IChapterContextFactory chapterFactory,
        ComputeAnchorsCommand anchorsCommand,
        BuildTranscriptIndexCommand transcriptCommand,
        HydrateTranscriptCommand hydrateCommand)
    {
        ArgumentNullException.ThrowIfNull(chapterFactory);
        ArgumentNullException.ThrowIfNull(anchorsCommand);
        ArgumentNullException.ThrowIfNull(transcriptCommand);
        ArgumentNullException.ThrowIfNull(hydrateCommand);

        var align = new Command("align", "Alignment utilities");
        align.AddCommand(CreateAnchors(chapterFactory, anchorsCommand));
        align.AddCommand(CreateTranscriptIndex(chapterFactory, transcriptCommand));
        align.AddCommand(CreateHydrateTx(chapterFactory, hydrateCommand));
        return align;
    }

    private static Command CreateAnchors(IChapterContextFactory factory, ComputeAnchorsCommand command)
    {
        var cmd = new Command("anchors", "Compute n-gram anchors between BookIndex and ASR");

        var indexOption = new Option<FileInfo?>("--index", "Path to BookIndex JSON") { IsRequired = true };
        indexOption.AddAlias("-i");
        var asrOption = new Option<FileInfo?>("--asr-json", "Path to ASR JSON") { IsRequired = true };
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
            try
            {
                var parse = context.ParseResult;
                var indexFile = CommandInputResolver.ResolveBookIndex(parse.GetValueForOption(indexOption));
                var asrFile = CommandInputResolver.ResolveChapterArtifact(parse.GetValueForOption(asrOption), "asr.json");
                var outFile = CommandInputResolver.ResolveChapterArtifact(parse.GetValueForOption(outOption), "align.anchors.json", mustExist: false);

                var options = new AnchorComputationOptions
                {
                    DetectSection = parse.GetValueForOption(detectSectionOption),
                    NGram = parse.GetValueForOption(ngramOption),
                    TargetPerTokens = parse.GetValueForOption(targetPerTokensOption),
                    MinSeparation = parse.GetValueForOption(minSeparationOption),
                    AllowBoundaryCross = parse.GetValueForOption(crossSentencesOption),
                    UseDomainStopwords = parse.GetValueForOption(domainStopwordsOption),
                    AsrPrefixTokens = parse.GetValueForOption(asrPrefixTokensOption),
                    EmitWindows = parse.GetValueForOption(emitWindowsOption)
                };

                using var handle = factory.Create(indexFile, asrFile: asrFile);
                await command.ExecuteAsync(handle.Chapter, options, context.GetCancellationToken()).ConfigureAwait(false);
                handle.Save();
                CopyIfRequested(handle.Chapter.ResolveArtifactFile("align.anchors.json"), outFile);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "align anchors command failed");
                context.ExitCode = 1;
            }
        });

        return cmd;
    }

    private static Command CreateTranscriptIndex(IChapterContextFactory factory, BuildTranscriptIndexCommand command)
    {
        var cmd = new Command("tx", "Validate & compare Book vs ASR using anchors; emit TranscriptIndex (*.tx.json)");

        var indexOption = new Option<FileInfo?>("--index", "Path to BookIndex JSON") { IsRequired = true };
        indexOption.AddAlias("-i");
        var asrOption = new Option<FileInfo?>("--asr-json", "Path to ASR JSON") { IsRequired = true };
        asrOption.AddAlias("-j");
        var audioOption = new Option<FileInfo?>("--audio", "Chapter audio file") { IsRequired = true };
        audioOption.AddAlias("-a");
        var outOption = new Option<FileInfo?>("--out", "Output TranscriptIndex JSON");
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
            try
            {
                var parse = context.ParseResult;
                var indexFile = CommandInputResolver.ResolveBookIndex(parse.GetValueForOption(indexOption));
                var asrFile = CommandInputResolver.ResolveChapterArtifact(parse.GetValueForOption(asrOption), "asr.json");
                var audioFile = CommandInputResolver.RequireAudio(parse.GetValueForOption(audioOption));
                var outFile = CommandInputResolver.ResolveChapterArtifact(parse.GetValueForOption(outOption), "align.tx.json", mustExist: false);

                var anchorOptions = new AnchorComputationOptions
                {
                    DetectSection = parse.GetValueForOption(detectSectionOption),
                    AsrPrefixTokens = parse.GetValueForOption(asrPrefixTokensOption),
                    NGram = parse.GetValueForOption(ngramOption),
                    TargetPerTokens = parse.GetValueForOption(targetPerTokensOption),
                    MinSeparation = parse.GetValueForOption(minSeparationOption),
                    AllowBoundaryCross = parse.GetValueForOption(crossSentencesOption),
                    UseDomainStopwords = parse.GetValueForOption(domainStopwordsOption)
                };

                var options = new BuildTranscriptIndexOptions
                {
                    AudioFile = audioFile,
                    AsrFile = asrFile,
                    BookIndexFile = indexFile,
                    AnchorOptions = anchorOptions
                };

                using var handle = factory.Create(indexFile, asrFile: asrFile, audioFile: audioFile);
                await command.ExecuteAsync(handle.Chapter, options, context.GetCancellationToken()).ConfigureAwait(false);
                handle.Save();
                CopyIfRequested(handle.Chapter.ResolveArtifactFile("align.tx.json"), outFile);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "align tx command failed");
                context.ExitCode = 1;
            }
        });

        return cmd;
    }

    private static Command CreateHydrateTx(IChapterContextFactory factory, HydrateTranscriptCommand command)
    {
        var cmd = new Command("hydrate", "Hydrate a TranscriptIndex with token values from BookIndex and ASR (for debugging)");

        var indexOption = new Option<FileInfo?>("--index", "Path to BookIndex JSON") { IsRequired = true };
        indexOption.AddAlias("-i");
        var asrOption = new Option<FileInfo?>("--asr-json", "Path to ASR JSON") { IsRequired = true };
        asrOption.AddAlias("-j");
        var txOption = new Option<FileInfo?>("--tx-json", "Path to TranscriptIndex JSON") { IsRequired = true };
        txOption.AddAlias("-t");
        var outOption = new Option<FileInfo?>("--out", "Output hydrated JSON file");
        outOption.AddAlias("-o");

        cmd.AddOption(indexOption);
        cmd.AddOption(asrOption);
        cmd.AddOption(txOption);
        cmd.AddOption(outOption);

        cmd.SetHandler(async context =>
        {
            try
            {
                var parse = context.ParseResult;
                var indexFile = CommandInputResolver.ResolveBookIndex(parse.GetValueForOption(indexOption));
                var asrFile = CommandInputResolver.ResolveChapterArtifact(parse.GetValueForOption(asrOption), "asr.json");
                var txFile = CommandInputResolver.ResolveChapterArtifact(parse.GetValueForOption(txOption), "align.tx.json");
                var outFile = CommandInputResolver.ResolveChapterArtifact(parse.GetValueForOption(outOption), "align.hydrate.json", mustExist: false);

                using var handle = factory.Create(indexFile, asrFile: asrFile, transcriptFile: txFile);
                await command.ExecuteAsync(handle.Chapter, null, context.GetCancellationToken()).ConfigureAwait(false);
                handle.Save();
                CopyIfRequested(handle.Chapter.ResolveArtifactFile("align.hydrate.json"), outFile);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "align hydrate command failed");
                context.ExitCode = 1;
            }
        });

        return cmd;
    }

    private static void CopyIfRequested(FileInfo source, FileInfo? destination)
    {
        if (destination is null)
        {
            return;
        }

        Directory.CreateDirectory(destination.Directory?.FullName ?? Directory.GetCurrentDirectory());
        File.Copy(source.FullName, destination.FullName, overwrite: true);
        Log.Debug("Wrote {Path}", destination.FullName);
    }
}
