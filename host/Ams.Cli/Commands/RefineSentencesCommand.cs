using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.CommandLine;
using System.Globalization;
using System.Text.Json;
using Ams.Core;
using Ams.Core.Align;
using Ams.Core.Pipeline;

public static class RefineSentencesCommand
{
    public static Command Create()
    {
        var cmd = new Command("refine-sentences", "Refine sentence start/end times using chunk alignments and silence timeline");

        var txOption = new Option<FileInfo>("--tx-json", description: "TranscriptIndex JSON") { IsRequired = true };
        txOption.AddAlias("-t");
        var asrOption = new Option<FileInfo>("--asr-json", description: "ASR JSON used by TX") { IsRequired = true };
        asrOption.AddAlias("-j");
        var audioOption = new Option<FileInfo>("--audio", description: "Audio WAV") { IsRequired = true };
        audioOption.AddAlias("-a");
        var outOption = new Option<FileInfo>("--out", description: "Output refined sentence JSON") { IsRequired = true };
        outOption.AddAlias("-o");
        var langOption = new Option<string>("--language", () => "eng", "Aeneas language code");
        var useSilenceOption = new Option<bool>("--with-silence", () => true, "Use silence events to adjust sentence ends");
        var silenceThreshOption = new Option<double>("--silence-threshold-db", () => -30.0, "Silence threshold in dBFS (legacy) ");
        silenceThreshOption.AddAlias("--db-floor");
        var silenceMinDurOption = new Option<double>("--silence-min-dur", () => 0.1, "Minimum silence duration in seconds (legacy)");
        silenceMinDurOption.AddAlias("--min-dur");
        var workOption = new Option<DirectoryInfo?>("--work", "Pipeline work directory containing book.index.json, anchors, align-chunks, timeline");
        var bookIndexOption = new Option<FileInfo?>("--book-index", "Path to book.index.json");
        var anchorsOption = new Option<FileInfo?>("--anchors-json", "Path to anchors/anchors.json");
        var alignmentsOption = new Option<DirectoryInfo?>("--alignments-dir", "Directory containing align-chunks/*.aeneas.json");
        var silenceJsonOption = new Option<FileInfo?>("--silence-json", "Path to timeline/silence.json");

        cmd.AddOption(txOption);
        cmd.AddOption(asrOption);
        cmd.AddOption(audioOption);
        cmd.AddOption(outOption);
        cmd.AddOption(langOption);
        cmd.AddOption(useSilenceOption);
        cmd.AddOption(silenceThreshOption);
        cmd.AddOption(silenceMinDurOption);
        cmd.AddOption(workOption);
        cmd.AddOption(bookIndexOption);
        cmd.AddOption(anchorsOption);
        cmd.AddOption(alignmentsOption);
        cmd.AddOption(silenceJsonOption);

        cmd.SetHandler(async context =>
        {
            var txFile = context.ParseResult.GetValueForOption(txOption)!;
            var asrFile = context.ParseResult.GetValueForOption(asrOption)!;
            var audioFile = context.ParseResult.GetValueForOption(audioOption)!;
            var outFile = context.ParseResult.GetValueForOption(outOption)!;
            var lang = context.ParseResult.GetValueForOption(langOption)!;
            var withSilence = context.ParseResult.GetValueForOption(useSilenceOption);
            var silenceDb = context.ParseResult.GetValueForOption(silenceThreshOption);
            var silenceMin = context.ParseResult.GetValueForOption(silenceMinDurOption);
            var workDir = context.ParseResult.GetValueForOption(workOption);
            var bookIndexFile = context.ParseResult.GetValueForOption(bookIndexOption);
            var anchorsFile = context.ParseResult.GetValueForOption(anchorsOption);
            var alignmentsDir = context.ParseResult.GetValueForOption(alignmentsOption);
            var silenceJsonFile = context.ParseResult.GetValueForOption(silenceJsonOption);

            try
            {
                await RunAsync(txFile, asrFile, audioFile, outFile, lang, withSilence, silenceDb, silenceMin, workDir, bookIndexFile, anchorsFile, alignmentsDir, silenceJsonFile);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        });

        return cmd;
    }

    private static async Task RunAsync(
        FileInfo txFile,
        FileInfo asrFile,
        FileInfo audioFile,
        FileInfo outFile,
        string language,
        bool withSilence,
        double silenceDb,
        double silenceMin,
        DirectoryInfo? workDir,
        FileInfo? bookIndexFile,
        FileInfo? anchorsFile,
        DirectoryInfo? alignmentsDir,
        FileInfo? silenceFile)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        if (!txFile.Exists)
        {
            throw new FileNotFoundException("TranscriptIndex JSON not found", txFile.FullName);
        }

        var asr = JsonSerializer.Deserialize<AsrResponse>(await File.ReadAllTextAsync(asrFile.FullName), jsonOptions)
                 ?? throw new InvalidOperationException("Failed to read ASR");

        var effectiveWorkDir = workDir?.FullName
            ?? bookIndexFile?.DirectoryName
            ?? txFile.DirectoryName
            ?? Directory.GetCurrentDirectory();

        bookIndexFile ??= new FileInfo(Path.Combine(effectiveWorkDir, "book.index.json"));
        if (!bookIndexFile.Exists)
            throw new InvalidOperationException($"Book index not found at {bookIndexFile.FullName}");

        var bookIndex = JsonSerializer.Deserialize<BookIndex>(await File.ReadAllTextAsync(bookIndexFile.FullName), jsonOptions)
                        ?? throw new InvalidOperationException("Failed to read BookIndex");

        var sectionRange = SentenceRefinementPreparation.TryLoadSectionWordRange(effectiveWorkDir, bookIndex);
        if (sectionRange is null && anchorsFile is { Exists: true })
        {
            var anchorsWorkDir = anchorsFile.Directory?.Parent?.FullName;
            if (!string.IsNullOrEmpty(anchorsWorkDir))
            {
                sectionRange = SentenceRefinementPreparation.TryLoadSectionWordRange(anchorsWorkDir, bookIndex);
            }
        }

        if (sectionRange is { } range)
        {
            Console.WriteLine($"[refine-cli] using section window words {range.Start}..{range.End}");
        }

        var (_, transcriptIndex, mapping) = SentenceRefinementPreparation.BuildTranscriptArtifacts(
            bookIndex,
            asr,
            audioFile.FullName,
            bookIndexFile.FullName,
            sectionRange);

        var alignmentsDirPath = alignmentsDir?.FullName ?? Path.Combine(effectiveWorkDir, "align-chunks", "chunks");
        var chunkAlignments = await SentenceRefinementPreparation.LoadChunkAlignmentsAsync(
            alignmentsDirPath,
            jsonOptions,
            CancellationToken.None);
        var chapterAlignmentIndex = ChapterAlignmentIndex.Build(chunkAlignments, mapping);

        var silencePath = silenceFile?.FullName ?? Path.Combine(effectiveWorkDir, "timeline", "silence.json");
        var silences = await SentenceRefinementPreparation.LoadSilencesAsync(
            silencePath,
            jsonOptions,
            CancellationToken.None);
        if (!withSilence)
        {
            Console.WriteLine("[refine-cli] silence snapping disabled; fragments will use ASR token bounds");
            silences = Array.Empty<SilenceEvent>();
        }

        var fragments = chapterAlignmentIndex.Fragments.ToDictionary(
            kvp => kvp.Key.ToString(CultureInfo.InvariantCulture),
            kvp => kvp.Value);

        _ = language;
        _ = silenceDb;

        var context = new SentenceRefinementContext(
            Fragments: fragments,
            Silences: silences,
            MinTailSec: Math.Max(0.05, silenceMin),
            MaxSnapAheadSec: withSilence ? 1.0 : 0.0);

        var svc = new SentenceRefinementService();
        var refined = await svc.RefineAsync(
            audioFile.FullName,
            transcriptIndex,
            asr,
            context,
            CancellationToken.None);

        var outputOptions = new JsonSerializerOptions { WriteIndented = true };
        Directory.CreateDirectory(outFile.DirectoryName!);
        await File.WriteAllTextAsync(outFile.FullName, JsonSerializer.Serialize(refined, outputOptions));
        Console.WriteLine($"Refined sentences written: {outFile.FullName}");
    }
}











