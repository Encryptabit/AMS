using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Threading;
using Ams.Core.Artifacts;
using Ams.Core.Audio;
using Ams.Core.Book;
using Ams.Core.Common;
using Ams.Core.Hydrate;
using Ams.Core.Prosody;
using Ams.Cli.Repl;
using Ams.Cli.Utilities;
using SentenceTiming = Ams.Core.Artifacts.SentenceTiming;

namespace Ams.Cli.Commands;

public static class ValidateCommand
{
    public static Command Create()
    {
        var validate = new Command("validate", "Validation utilities");
        validate.AddCommand(CreateReportCommand());
        validate.AddCommand(CreateTimingCommand());
        validate.AddCommand(CreateTimingApplyCommand());
        validate.AddCommand(CreateServeCommand());
        return validate;
    }

    private static Command CreateTimingCommand()
    {
        var cmd = new Command("timing", "Interactively review and adjust sentence spacing gaps");

        var txOption = new Option<FileInfo?>(
            name: "--tx",
            description: "Path to TranscriptIndex JSON (defaults to active chapter's *.align.tx.json)");
        txOption.AddAlias("-t");

        var hydrateOption = new Option<FileInfo?>(
            name: "--hydrate",
            description: "Path to hydrated transcript JSON (defaults to active chapter's *.align.hydrate.json)");
        hydrateOption.AddAlias("-h");

        var bookIndexOption = new Option<FileInfo?>(
            name: "--book-index",
            description: "Path to book-index.json (defaults to working directory)");

        var prosodyAnalyzeOption = new Option<bool>(
            "--prosody-analyze",
            () => true,
            "Run pause dynamics analysis before launching the interactive session.");

        var useAdjustedOption = new Option<bool>(
            "--use-adjusted",
            () => false,
            "Load pause-adjusted transcript/hydrate artifacts when present.");

        var includeAllIntraOption = new Option<bool>(
            "--all-gaps",
            () => false,
            "Include all detected intra-sentence gaps (TextGrid silences) instead of only script punctuation.");
        includeAllIntraOption.AddAlias("-A");

        var interOnlyOption = new Option<bool>(
            "--inter-only",
            () => false,
            "Only allow inter-sentence (between sentences) pause edits; ignore intra-sentence adjustments.");

        var headlessOption = new Option<bool>(
            "--headless",
            () => false,
            "Apply default pause compression and automatically run timing-apply without launching the UI.");

        var headlessOverwriteOption = new Option<bool>(
            "--overwrite",
            () => false,
            "Overwrite pause-adjusted outputs when --headless runs timing-apply.");

        cmd.AddOption(txOption);
        cmd.AddOption(hydrateOption);
        cmd.AddOption(bookIndexOption);
        cmd.AddOption(prosodyAnalyzeOption);
        cmd.AddOption(useAdjustedOption);
        cmd.AddOption(includeAllIntraOption);
        cmd.AddOption(interOnlyOption);
        cmd.AddOption(headlessOption);
        cmd.AddOption(headlessOverwriteOption);

        cmd.AddCommand(CreateTimingInitCommand());

        cmd.SetHandler(async context =>
        {
            var tx = CommandInputResolver.TryResolveChapterArtifact(
                context.ParseResult.GetValueForOption(txOption),
                suffix: "align.tx.json",
                mustExist: true);

            if (tx is null)
            {
                Log.Error("validate timing requires --tx or an active chapter with TranscriptIndex JSON");
                context.ExitCode = 1;
                return;
            }

            try
            {
                var cancellationToken = context.GetCancellationToken();

                var hydrate = CommandInputResolver.TryResolveChapterArtifact(
                    context.ParseResult.GetValueForOption(hydrateOption),
                    suffix: "align.hydrate.json",
                    mustExist: true);

                if (hydrate is null || !hydrate.Exists)
                {
                    Log.Error("validate timing requires a hydrated transcript JSON (e.g., *.align.hydrate.json)");
                    context.ExitCode = 1;
                    return;
                }

                var useAdjusted = context.ParseResult.GetValueForOption(useAdjustedOption);
                if (useAdjusted)
                {
                    var adjustedTx = TryResolveAdjustedArtifact(tx, ".pause-adjusted.align.tx.json");
                    if (adjustedTx is not null)
                    {
                        Log.Debug("validate timing loading pause-adjusted transcript: {0}", adjustedTx.FullName);
                        tx = adjustedTx;
                    }
                    else
                    {
                        Log.Debug("Pause-adjusted transcript not found; falling back to {0}", tx.FullName);
                    }

                    var adjustedHydrate = TryResolveAdjustedArtifact(hydrate, ".pause-adjusted.align.hydrate.json");
                    if (adjustedHydrate is not null)
                    {
                        Log.Debug("validate timing loading pause-adjusted hydrate: {0}", adjustedHydrate.FullName);
                        hydrate = adjustedHydrate;
                    }
                    else
                    {
                        Log.Debug("Pause-adjusted hydrate not found; falling back to {0}", hydrate.FullName);
                    }
                }

                var headless = context.ParseResult.GetValueForOption(headlessOption);
                FileInfo? bookIndexOverride = context.ParseResult.GetValueForOption(bookIndexOption);
                FileInfo bookIndex;
                try
                {
                    bookIndex = CommandInputResolver.ResolveBookIndex(
                        bookIndexOverride,
                        mustExist: true);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "validate timing failed to locate book-index.json");
                    context.ExitCode = 1;
                    return;
                }

                var runProsody = context.ParseResult.GetValueForOption(prosodyAnalyzeOption);
                var includeAllIntra = context.ParseResult.GetValueForOption(includeAllIntraOption);
                var interOnly = context.ParseResult.GetValueForOption(interOnlyOption);

                var session = new ValidateTimingSession(tx, bookIndex, hydrate, runProsody, includeAllIntra, interOnly);

                if (headless)
                {
                    var overwriteOutputs = context.ParseResult.GetValueForOption(headlessOverwriteOption);
                    var headlessResult = await session.RunHeadlessAsync(cancellationToken).ConfigureAwait(false);
                    if (!headlessResult.HasAdjustments)
                    {
                        context.ExitCode = 0;
                        return;
                    }

                    Log.Debug(
                        "validate timing headless committed {Adjustments} adjustment(s); invoking timing-apply",
                        headlessResult.AdjustmentCount);

                    var applyExitCode = RunTimingApplyCore(
                        txFile: tx,
                        hydrateFile: hydrate,
                        adjustmentsOverride: headlessResult.AdjustmentsFile,
                        outWavOverride: null,
                        roomtoneOverride: null,
                        bookIndexOverride: bookIndexOverride,
                        toneGainDb: -60.0,
                        overwriteOutputs: overwriteOutputs,
                        useAdjusted: false,
                        cancellationToken: cancellationToken);

                    context.ExitCode = applyExitCode;
                    return;
                }

                await session.RunAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Log.Debug("validate timing cancelled");
                context.ExitCode = 1;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "validate timing failed");
                context.ExitCode = 1;
            }
        });

        return cmd;
    }

    private static Command CreateTimingInitCommand()
    {
        var cmd = new Command("init", "Scaffold a pause-policy.json for the current chapter or entire book.");

        var bookOption = new Option<bool>(
            "--book",
            () => false,
            "Create or overwrite the book-level pause-policy.json in the working directory.");

        var chapterOption = new Option<bool>(
            "--chapter",
            () => false,
            "Create or overwrite the active chapter's pause-policy.json (default).");

        var forceOption = new Option<bool>(
            "--force",
            () => false,
            "Overwrite an existing pause-policy.json if present.");

        cmd.AddOption(bookOption);
        cmd.AddOption(chapterOption);
        cmd.AddOption(forceOption);

        cmd.SetHandler(context =>
        {
            bool book = context.ParseResult.GetValueForOption(bookOption);
            bool chapter = context.ParseResult.GetValueForOption(chapterOption);
            bool force = context.ParseResult.GetValueForOption(forceOption);

            if (book && chapter)
            {
                Log.Error("Specify at most one of --book or --chapter.");
                context.ExitCode = 1;
                return;
            }

            if (!book && !chapter)
            {
                chapter = true;
            }

            string? targetPath = null;

            if (book)
            {
                var workingDir = ReplContext.Current?.WorkingDirectory ?? Directory.GetCurrentDirectory();
                targetPath = Path.Combine(workingDir, "pause-policy.json");
            }
            else
            {
                var repl = ReplContext.Current;
                if (repl?.ActiveChapterStem is null)
                {
                    Log.Error("validate timing init --chapter requires an active chapter. Use 'use <chapter>' first or pass --book.");
                    context.ExitCode = 1;
                    return;
                }

                var chapterDir = Path.Combine(repl.WorkingDirectory, repl.ActiveChapterStem);
                Directory.CreateDirectory(chapterDir);
                targetPath = Path.Combine(chapterDir, "pause-policy.json");
            }

            if (string.IsNullOrWhiteSpace(targetPath))
            {
                Log.Error("Failed to determine pause-policy.json destination.");
                context.ExitCode = 1;
                return;
            }

            if (File.Exists(targetPath) && !force)
            {
                Log.Debug("pause-policy.json already exists at {Path}. Re-run with --force to overwrite.", targetPath);
                context.ExitCode = 0;
                return;
            }

            try
            {
                var policy = PausePolicyPresets.House();
                PausePolicyStorage.Save(targetPath, policy);
                Log.Debug("pause-policy.json written to {Path}", targetPath);
                context.ExitCode = 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to write pause-policy.json to {Path}", targetPath);
                context.ExitCode = 1;
            }
        });

        return cmd;
    }

    internal static int RunTimingApplyCore(
        FileInfo txFile,
        FileInfo hydrateFile,
        FileInfo? adjustmentsOverride,
        FileInfo? outWavOverride,
        FileInfo? roomtoneOverride,
        FileInfo? bookIndexOverride,
        double toneGainDb,
        bool overwriteOutputs,
        bool useAdjusted,
        CancellationToken cancellationToken)
    {
        if (txFile is null) throw new ArgumentNullException(nameof(txFile));
        if (hydrateFile is null) throw new ArgumentNullException(nameof(hydrateFile));

        var effectiveTx = txFile;
        var effectiveHydrate = hydrateFile;

        if (!effectiveTx.Exists)
        {
            Log.Error("TranscriptIndex JSON not found at {Path}", effectiveTx.FullName);
            return 1;
        }

        if (!effectiveHydrate.Exists)
        {
            Log.Error("Hydrated transcript JSON not found at {Path}", effectiveHydrate.FullName);
            return 1;
        }

        if (useAdjusted)
        {
            var adjustedTx = TryResolveAdjustedArtifact(txFile, ".pause-adjusted.align.tx.json");
            if (adjustedTx is not null)
            {
                Log.Debug("timing-apply loading pause-adjusted transcript: {0}", adjustedTx.FullName);
                effectiveTx = adjustedTx;
            }
            else
            {
                Log.Debug("Pause-adjusted transcript not found; using {0}", effectiveTx.FullName);
            }

            var adjustedHydrate = TryResolveAdjustedArtifact(hydrateFile, ".pause-adjusted.align.hydrate.json");
            if (adjustedHydrate is not null)
            {
                Log.Debug("timing-apply loading pause-adjusted hydrate: {0}", adjustedHydrate.FullName);
                effectiveHydrate = adjustedHydrate;
            }
            else
            {
                Log.Debug("Pause-adjusted hydrate not found; using {0}", effectiveHydrate.FullName);
            }
        }

        var adjustmentsFile = adjustmentsOverride ?? ResolveAdjustmentsPath(effectiveTx, null);
        if (!adjustmentsFile.Exists)
        {
            Log.Error("Pause adjustments file not found: {0}", adjustmentsFile.FullName);
            return 1;
        }

        PauseAdjustmentsDocument adjustments;
        try
        {
            adjustments = PauseAdjustmentsDocument.Load(adjustmentsFile.FullName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load pause adjustments from {Path}", adjustmentsFile.FullName);
            return 1;
        }

        if (adjustments.Adjustments.Count == 0)
        {
            Log.Debug("No pause adjustments found in {Path}. Nothing to apply.", adjustmentsFile.FullName);
            return 0;
        }

        var transcript = LoadJson<TranscriptIndex>(effectiveTx);
        var hydrated = LoadJson<HydratedTranscript>(effectiveHydrate);

        FileInfo? bookIndexFile = bookIndexOverride ?? TryResolveBookIndex(transcript.BookIndexPath, effectiveTx.DirectoryName);
        BookIndex? bookIndex = null;
        if (bookIndexFile is not null && bookIndexFile.Exists)
        {
            bookIndex = LoadJson<BookIndex>(bookIndexFile);
        }

        var audioPath = ResolveAudioPath(transcript, hydrated, effectiveTx, effectiveHydrate);
        if (!File.Exists(audioPath))
        {
            Log.Error("Audio file not found at {Path}", audioPath);
            return 1;
        }

        string roomtonePath;
        double? stageFadeMs;
        double? stageAppliedGainDb;
        try
        {
            roomtonePath = ResolveRoomtonePath(
                roomtoneOverride,
                effectiveTx,
                transcript,
                hydrated,
                audioPath,
                transcript.ScriptPath,
                effectiveHydrate.DirectoryName ?? effectiveTx.DirectoryName,
                bookIndex,
                out stageFadeMs,
                out stageAppliedGainDb);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to locate roomtone.wav");
            return 1;
        }

        var outWav = outWavOverride ?? BuildSiblingFile(audioPath, ".pause-adjusted.wav");
        try
        {
            EnsureWritable(outWav, overwriteOutputs);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Cannot write output WAV to {Path}", outWav.FullName);
            return 1;
        }

        cancellationToken.ThrowIfCancellationRequested();

        var audioBuffer = WavIo.ReadPcmOrFloat(audioPath);
        var roomtoneBuffer = WavIo.ReadPcmOrFloat(roomtonePath);
        double fadeMs = stageFadeMs ?? 5.0;

        double targetToneDb = toneGainDb;
        var toneAnalyzer = new AudioAnalysisService(roomtoneBuffer);
        double toneDuration = roomtoneBuffer.SampleRate > 0 ? roomtoneBuffer.Length / (double)roomtoneBuffer.SampleRate : 0.0;
        var toneStats = toneAnalyzer.AnalyzeGap(0.0, toneDuration);
        double toneGainLinear = 1.0;
        Log.Debug(
            "timing-apply using roomtone seed {0} (stageFade={1}, stageGain={2}, measuredRms={3:F2} dB, targetRms={4:F2} dB)",
            roomtonePath,
            stageFadeMs,
            stageAppliedGainDb,
            toneStats.MeanRmsDb,
            targetToneDb);

        var baselineTimings = BuildBaselineTimings(transcript, hydrated);

        var vettedAdjustments = VetPauseAdjustments(adjustments.Adjustments, transcript, audioBuffer);
        var timelineResult = PauseTimelineApplier.Apply(baselineTimings, vettedAdjustments);

        var adjustedAudio = PauseAudioApplier.Apply(
            audioBuffer,
            roomtoneBuffer,
            transcript.Sentences,
            timelineResult.Timeline,
            toneGainLinear,
            fadeMs: fadeMs,
            intraSentenceGaps: timelineResult.IntraSentenceGaps);
        WavIo.WriteFloat32(outWav.FullName, adjustedAudio);

    var updatedTranscript = UpdateTranscriptTimings(transcript, timelineResult.Timeline);
        var updatedHydrate = UpdateHydratedTimings(hydrated, timelineResult.Timeline);

        var transcriptOut = BuildOutputJsonPath(effectiveTx, ".pause-adjusted.align.tx.json");
        var hydrateOut = BuildOutputJsonPath(effectiveHydrate, ".pause-adjusted.align.hydrate.json");

        try
        {
            EnsureWritable(transcriptOut, overwriteOutputs);
            EnsureWritable(hydrateOut, overwriteOutputs);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Cannot write updated timing JSON");
            return 1;
        }

        SaveJson(transcriptOut, updatedTranscript);
        SaveJson(hydrateOut, updatedHydrate);

        Log.Debug("Pause-adjusted audio saved to {Output}", outWav.FullName);
        Log.Debug("Updated transcript timings saved to {Output}", transcriptOut.FullName);
        Log.Debug("Updated hydrate timings saved to {Output}", hydrateOut.FullName);

        return 0;
    }

    private static Command CreateTimingApplyCommand()
    {
        var cmd = new Command("timing-apply", "Render pause-adjusted audio and refreshed timing JSON from saved adjustments");

        var txOption = new Option<FileInfo?>("--tx", "Path to TranscriptIndex JSON (*.align.tx.json)");
        txOption.AddAlias("-t");

        var hydrateOption = new Option<FileInfo?>("--hydrate", "Path to hydrated transcript JSON (*.align.hydrate.json)");
        hydrateOption.AddAlias("-h");

        var adjustmentsOption = new Option<FileInfo?>("--adjustments", "Path to pause-adjustments JSON (defaults to <chapter>.pause-adjustments.json)");
        var outWavOption = new Option<FileInfo?>("--out-wav", "Output WAV path (defaults to <audio>.pause-adjusted.wav)");
        var roomtoneOption = new Option<FileInfo?>("--roomtone", "Override roomtone WAV path (defaults to roomtone.wav near chapter)");
        var bookIndexOption = new Option<FileInfo?>("--book-index", "Optional book-index.json path when discovery via TranscriptIndex fails");
        var toneGainOption = new Option<double>("--tone-gain-db", () => -60.0, "Target RMS level for injected roomtone (dBFS)");
        var overwriteOption = new Option<bool>("--overwrite", () => false, "Overwrite existing outputs");
        var useAdjustedOption = new Option<bool>("--use-adjusted", () => false, "Load pause-adjusted transcript/hydrate artifacts when present");

        cmd.AddOption(txOption);
        cmd.AddOption(hydrateOption);
        cmd.AddOption(adjustmentsOption);
        cmd.AddOption(outWavOption);
        cmd.AddOption(roomtoneOption);
        cmd.AddOption(bookIndexOption);
        cmd.AddOption(toneGainOption);
        cmd.AddOption(overwriteOption);
        cmd.AddOption(useAdjustedOption);

        cmd.SetHandler(context =>
        {
            try
            {
                var cancellationToken = context.GetCancellationToken();

                var txFile = CommandInputResolver.ResolveChapterArtifact(
                    context.ParseResult.GetValueForOption(txOption),
                    suffix: "align.tx.json",
                    mustExist: true);

                if (txFile is null)
                {
                    Log.Error("timing-apply requires --tx or an active chapter with TranscriptIndex JSON");
                    context.ExitCode = 1;
                    return;
                }

                var hydrateFile = CommandInputResolver.ResolveChapterArtifact(
                    context.ParseResult.GetValueForOption(hydrateOption),
                    suffix: "align.hydrate.json",
                    mustExist: true);

                if (hydrateFile is null || !hydrateFile.Exists)
                {
                    Log.Error("timing-apply requires a hydrated transcript JSON (e.g., *.align.hydrate.json)");
                    context.ExitCode = 1;
                    return;
                }

                var adjustmentsOverride = context.ParseResult.GetValueForOption(adjustmentsOption);
                var outWavOverride = context.ParseResult.GetValueForOption(outWavOption);
                var roomtoneOverride = context.ParseResult.GetValueForOption(roomtoneOption);
                var bookIndexOverride = context.ParseResult.GetValueForOption(bookIndexOption);
                var toneGainDb = context.ParseResult.GetValueForOption(toneGainOption);
                var overwrite = context.ParseResult.GetValueForOption(overwriteOption);
                var useAdjusted = context.ParseResult.GetValueForOption(useAdjustedOption);

                context.ExitCode = RunTimingApplyCore(
                    txFile,
                    hydrateFile,
                    adjustmentsOverride,
                    outWavOverride,
                    roomtoneOverride,
                    bookIndexOverride,
                    toneGainDb,
                    overwrite,
                    useAdjusted,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Log.Debug("timing-apply cancelled");
                context.ExitCode = 1;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "timing-apply failed");
                context.ExitCode = 1;
            }
        });

        return cmd;
    }

    private static Command CreateServeCommand()
    {
        var serve = new Command("serve", "Start web viewer for validation reports");
        
        var workDirOption = new Option<DirectoryInfo?>(
            "--work-dir",
            "Directory containing chapter folders with validation reports (defaults to REPL working directory or current directory)");
        
        var portOption = new Option<int>(
            "--port",
            () => 8081,
            "Port to run the web server on");
        
        serve.AddOption(workDirOption);
        serve.AddOption(portOption);
        
        serve.SetHandler(async (InvocationContext context) =>
        {
            var workDirProvided = context.ParseResult.GetValueForOption(workDirOption);
            var port = context.ParseResult.GetValueForOption(portOption);
            var cancellationToken = context.GetCancellationToken();
            
            var workDir = CommandInputResolver.ResolveDirectory(workDirProvided);
            var baseDir = workDir.FullName;
            
            if (!Directory.Exists(baseDir))
            {
                Log.Error("Work directory not found: {WorkDir}", baseDir);
                context.ExitCode = 1;
                return;
            }
            
            Log.Debug("Starting validation report viewer...");
            Log.Debug("Base directory: {BaseDir}", baseDir);
            Log.Debug("Server will run at: http://localhost:{Port}", port);
            
            var pythonScript = Path.Combine(
                AppContext.BaseDirectory, 
                "..", "..", "..", "..", "..",
                "tools", "validation-viewer", "server.py");
            
            if (!File.Exists(pythonScript))
            {
                Log.Error("Validation viewer script not found at: {Path}", pythonScript);
                Log.Debug("Expected location: {Path}", pythonScript);
                context.ExitCode = 1;
                return;
            }
            
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                UseShellExecute = false,
                CreateNoWindow = false
            };
            
            psi.ArgumentList.Add(pythonScript);
            psi.ArgumentList.Add(baseDir);
            psi.ArgumentList.Add(port.ToString());
            
            using var process = Process.Start(psi);
            if (process == null)
            {
                Log.Error("Failed to start validation viewer");
                context.ExitCode = 1;
                return;
            }
            
            Log.Debug("Validation viewer started. Press Ctrl+C to stop.");
            
            try
            {
                await process.WaitForExitAsync(cancellationToken);
                context.ExitCode = process.ExitCode;
            }
            catch (OperationCanceledException)
            {
                Log.Debug("Shutting down validation viewer...");
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
                context.ExitCode = 0;
            }
        });
        
        return serve;
    }

    private static Command CreateReportCommand()
    {
        var cmd = new Command("report", "Render a human-friendly view of transcript validation metrics");

        var txOption = new Option<FileInfo?>(
            name: "--tx",
            description: "Path to TranscriptIndex JSON (e.g., *.align.tx.json)");
        txOption.AddAlias("-t");

        var hydrateOption = new Option<FileInfo?>(
            name: "--hydrate",
            description: "Path to hydrated transcript JSON (e.g., *.align.hydrate.json)");
        hydrateOption.AddAlias("-h");

        var outOption = new Option<FileInfo?>("--out", () => null, "Optional output file. If omitted, prints to console or is derived from chapter.");
        outOption.AddAlias("-o");

        var allErrorsOption = new Option<bool>("--show-all", () => true, "Flag to determine whether to display all errors or not");
        var topSentencesOption = new Option<int>("--top-sentences", () => 10, "Number of highest-WER sentences to display (0 to disable).");
        var topParagraphsOption = new Option<int>("--top-paragraphs", () => 5, "Number of highest-WER paragraphs to display (0 to disable).");
        var includeWordsOption = new Option<bool>("--include-words", () => false, "Include word-level alignment tallies when TranscriptIndex is provided.");
        var includeAllFlaggedOption = new Option<bool>("--include-all-flagged", () => false, "Include parent paragraphs of all flagged sentences, even if the paragraph status is 'ok'.");

        cmd.AddOption(txOption);
        cmd.AddOption(hydrateOption);
        cmd.AddOption(outOption);
        cmd.AddOption(allErrorsOption);
        cmd.AddOption(topSentencesOption);
        cmd.AddOption(topParagraphsOption);
        cmd.AddOption(includeWordsOption);
        cmd.AddOption(includeAllFlaggedOption);

        cmd.SetHandler(async context =>
        {
            var txFile = CommandInputResolver.TryResolveChapterArtifact(context.ParseResult.GetValueForOption(txOption), "align.tx.json", mustExist: true);
            var hydrateFile = CommandInputResolver.TryResolveChapterArtifact(context.ParseResult.GetValueForOption(hydrateOption), "align.hydrate.json", mustExist: true);
            var outputFile = CommandInputResolver.TryResolveChapterArtifact(context.ParseResult.GetValueForOption(outOption), "validate.report.txt", mustExist: false);
            var allErrors = context.ParseResult.GetValueForOption(allErrorsOption);
            var topSentences = context.ParseResult.GetValueForOption(topSentencesOption);
            var topParagraphs = context.ParseResult.GetValueForOption(topParagraphsOption);
            var includeWords = context.ParseResult.GetValueForOption(includeWordsOption);
            var includeAllFlagged = context.ParseResult.GetValueForOption(includeAllFlaggedOption);

            if (txFile is null && hydrateFile is null)
            {
                Log.Error("validate report requires --tx, --hydrate, or both");
                context.ExitCode = 1;
                return;
            }

            try
            {
                var result = await GenerateReportAsync(txFile, hydrateFile, allErrors, topSentences, topParagraphs, includeWords, includeAllFlagged);

                var summarySentences = result.Sentences.Count;
                var summarySentencesFlagged = result.Sentences.Count(s => !string.Equals(s.Status, "ok", StringComparison.OrdinalIgnoreCase));
                var summaryParagraphs = result.Paragraphs.Count;
                var summaryParagraphsFlagged = result.Paragraphs.Count(p => !string.Equals(p.Status, "ok", StringComparison.OrdinalIgnoreCase));

                Console.WriteLine("=== Validation Summary ===");
                Console.WriteLine($"Sentences : {summarySentences} (flagged {summarySentencesFlagged})");
                Console.WriteLine($"Paragraphs: {summaryParagraphs} (flagged {summaryParagraphsFlagged})");
                Console.WriteLine();

                var targetFile = outputFile
                    ?? CommandInputResolver.TryResolveChapterArtifact(null, "validate.report.txt", mustExist: false)
                    ?? new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "validate.report.txt"));

                Directory.CreateDirectory(targetFile.DirectoryName ?? Directory.GetCurrentDirectory());
                await File.WriteAllTextAsync(targetFile.FullName, result.Report, Encoding.UTF8);
                Log.Debug("Validation report written to {Output}", targetFile.FullName);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "validate report failed");
                context.ExitCode = 1;
            }
        });

        return cmd;
    }

    private static async Task<ReportResult> GenerateReportAsync(FileInfo? txFile,
        FileInfo? hydrateFile,
        bool allErrors,
        int topSentences,
        int topParagraphs,
        bool includeWordTallies,
        bool includeAllFlagged)
    {
        TranscriptIndex? tx = null;
        HydratedTranscript? hydrated = null;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        if (txFile is not null)
        {
            if (!txFile.Exists)
            {
                throw new FileNotFoundException($"TranscriptIndex file not found: {txFile.FullName}");
            }

            var txJson = await File.ReadAllTextAsync(txFile.FullName);
            tx = JsonSerializer.Deserialize<TranscriptIndex>(txJson, jsonOptions)
                 ?? throw new InvalidOperationException("Failed to deserialize TranscriptIndex JSON");
        }

        if (hydrateFile is not null)
        {
            if (!hydrateFile.Exists)
            {
                throw new FileNotFoundException($"Hydrated transcript file not found: {hydrateFile.FullName}");
            }

            var hydrateJson = await File.ReadAllTextAsync(hydrateFile.FullName);
            hydrated = JsonSerializer.Deserialize<HydratedTranscript>(hydrateJson, jsonOptions)
                        ?? throw new InvalidOperationException("Failed to deserialize hydrated transcript JSON");
        }

        if (tx is null && hydrated is null)
        {
            throw new InvalidOperationException("No transcript artifacts could be loaded");
        }

        var info = ExtractSourceInfo(tx, hydrated);
        var sentenceViews = BuildSentenceViews(tx, hydrated);
        var paragraphViews = BuildParagraphViews(tx, hydrated);
        var wordTallies = includeWordTallies ? BuildWordTallies(tx) : null;

        var fullReport = BuildTextReport(info, sentenceViews, paragraphViews, wordTallies, allErrors, topSentences, topParagraphs, includeAllFlagged, hydrated);
        return new ReportResult(fullReport, sentenceViews, paragraphViews, wordTallies);
    }

    private static SourceInfo ExtractSourceInfo(TranscriptIndex? tx, HydratedTranscript? hydrated)
    {
        return tx is not null
            ? new SourceInfo(tx.AudioPath, tx.ScriptPath, tx.BookIndexPath, tx.CreatedAtUtc)
            : new SourceInfo(
                hydrated!.AudioPath,
                hydrated.ScriptPath,
                hydrated.BookIndexPath,
                hydrated.CreatedAtUtc);
    }

    private static IReadOnlyList<SentenceView> BuildSentenceViews(TranscriptIndex? tx, HydratedTranscript? hydrated)
    {
        if (tx is null && hydrated is null)
        {
            return Array.Empty<SentenceView>();
        }

        var txMap = tx?.Sentences.ToDictionary(s => s.Id);
        var hydratedMap = hydrated?.Sentences.ToDictionary(s => s.Id);

        var ids = new SortedSet<int>();
        if (txMap is not null)
        {
            foreach (var id in txMap.Keys)
            {
                ids.Add(id);
            }
        }
        if (hydratedMap is not null)
        {
            foreach (var id in hydratedMap.Keys)
            {
                ids.Add(id);
            }
        }

        var views = new List<SentenceView>(ids.Count);
        foreach (var id in ids)
        {
            SentenceAlign? txSentence = null;
            HydratedSentence? hydSentence = null;
            txMap?.TryGetValue(id, out txSentence);
            hydratedMap?.TryGetValue(id, out hydSentence);

            var bookRange = hydSentence is not null
                ? (hydSentence.BookRange.Start, hydSentence.BookRange.End)
                : txSentence is not null
                    ? (txSentence.BookRange.Start, txSentence.BookRange.End)
                    : (0, 0);

            var scriptRange = hydSentence?.ScriptRange is not null
                ? (hydSentence.ScriptRange.Start, hydSentence.ScriptRange.End)
                : txSentence?.ScriptRange is not null
                    ? (txSentence.ScriptRange.Start, txSentence.ScriptRange.End)
                    : (null, null);

            var metrics = txSentence?.Metrics ?? hydSentence?.Metrics
                ?? new SentenceMetrics(0, 0, 0, 0, 0);
            var status = hydSentence?.Status ?? txSentence?.Status ?? "unknown";
            string? bookText = hydSentence?.BookText;
            string? scriptText = hydSentence?.ScriptText;
            var timing = hydSentence?.Timing ?? txSentence?.Timing;

            views.Add(new SentenceView(
                id,
                bookRange,
                scriptRange,
                metrics,
                status,
                string.IsNullOrWhiteSpace(bookText) ? null : bookText,
                string.IsNullOrWhiteSpace(scriptText) ? null : scriptText,
                timing));
        }

        return views.OrderBy(s => s.Id).ToList();
    }

    private static IReadOnlyList<ParagraphView> BuildParagraphViews(TranscriptIndex? tx, HydratedTranscript? hydrated)
    {
        if (tx is null && hydrated is null)
        {
            return Array.Empty<ParagraphView>();
        }

        var paragraphs = hydrated?.Paragraphs ?? tx!.Paragraphs.Select(p => new HydratedParagraph(
            p.Id,
            new HydratedRange(p.BookRange.Start, p.BookRange.End),
            p.SentenceIds,
            BookText: string.Empty,
            p.Metrics,
            p.Status)).ToList();

        return paragraphs!
            .Select(p => new ParagraphView(
                p.Id,
                (p.BookRange.Start, p.BookRange.End),
                p.Metrics,
                p.Status,
                string.IsNullOrWhiteSpace(p.BookText) ? null : p.BookText))
            .OrderBy(p => p.Id)
            .ToList();
    }

    private static WordTallies? BuildWordTallies(TranscriptIndex? tx)
    {
        if (tx is null)
        {
            return null;
        }

        int match = 0, substitution = 0, insertion = 0, deletion = 0;

        foreach (var word in tx.Words)
        {
            switch (word.Op)
            {
                case AlignOp.Match:
                    match++;
                    break;
                case AlignOp.Sub:
                    substitution++;
                    break;
                case AlignOp.Ins:
                    insertion++;
                    break;
                case AlignOp.Del:
                    deletion++;
                    break;
            }
        }

        return new WordTallies(match, substitution, insertion, deletion, tx.Words.Count);
    }

    private static string BuildTextReport(
        SourceInfo info,
        IReadOnlyList<SentenceView> sentences,
        IReadOnlyList<ParagraphView> paragraphs,
        WordTallies? wordTallies,
        bool allErrors,
        int topSentences,
        int topParagraphs,
        bool includeAllFlagged,
        HydratedTranscript? hydrated)
    {
        var builder = new StringBuilder();

        builder.AppendLine("=== Validation Report ===");
        builder.AppendLine($"Audio     : {info.AudioPath}");
        builder.AppendLine($"Script    : {info.ScriptPath}");
        builder.AppendLine($"Book Index: {info.BookIndexPath}");
        builder.AppendLine($"Created   : {info.CreatedAtUtc:O}");
        builder.AppendLine();

        if (sentences.Count > 0)
        {
            var avgWer = sentences.Average(s => s.Metrics.Wer);
            var maxWer = sentences.Max(s => s.Metrics.Wer);
            var flagged = sentences.Count(s => !string.Equals(s.Status, "ok", StringComparison.OrdinalIgnoreCase));

            builder.AppendLine($"Sentences : {sentences.Count} (Avg WER {avgWer:P2}, Max WER {maxWer:P2}, Flagged {flagged})");
        }
        else
        {
            builder.AppendLine("Sentences : 0");
        }

        if (paragraphs.Count > 0)
        {
            var avgWer = paragraphs.Average(p => p.Metrics.Wer);
            var avgCoverage = paragraphs.Average(p => p.Metrics.Coverage);
            builder.AppendLine($"Paragraphs: {paragraphs.Count} (Avg WER {avgWer:P2}, Avg Coverage {avgCoverage:P2})");
        }
        else
        {
            builder.AppendLine("Paragraphs: 0");
        }

        if (wordTallies is not null)
        {
            builder.AppendLine($"Words     : {wordTallies.Total} (Match {wordTallies.Match}, Sub {wordTallies.Substitution}, Ins {wordTallies.Insertion}, Del {wordTallies.Deletion})");
        }

        builder.AppendLine();

        if ( topSentences > 0 && sentences.Count > 0)
        {
            if (allErrors) builder.AppendLine("All sentences by WER:");
            else builder.AppendLine($"Top {Math.Min(topSentences, sentences.Count)} sentences by WER:");

            var sentencesOrdered = sentences
                .OrderByDescending(s => s.Metrics.Wer)
                .ThenByDescending(s => s.Metrics.Cer);
            
            var sentenceBucket = allErrors ? sentencesOrdered.Where(s => !s.Status.Equals("ok", StringComparison.OrdinalIgnoreCase)) : sentencesOrdered.Take(topSentences);

            foreach (var sentence in sentenceBucket)
            {
                builder.AppendLine($"  #{sentence.Id} | WER {sentence.Metrics.Wer:P1} | CER {sentence.Metrics.Cer:P1} | Status {sentence.Status}");
                builder.AppendLine($"    Book range: {sentence.BookRange.Start}-{sentence.BookRange.End}");

                if (sentence.ScriptRange is not null)
                {
                    builder.AppendLine($"    Script range: {sentence.ScriptRange.Value.Start}-{sentence.ScriptRange.Value.End}");
                }

                if (sentence.Timing is not null)
                {
                    var timing = sentence.Timing;
                    builder.AppendLine($"    Timing: {timing.StartSec:F3}s → {timing.EndSec:F3}s (Δ {timing.Duration:F3}s)");
                }

                if (!string.IsNullOrWhiteSpace(sentence.BookText))
                {
                    builder.AppendLine($"    Book   : {TrimText(sentence.BookText)}");
                }

                if (!string.IsNullOrWhiteSpace(sentence.ScriptText))
                {
                    builder.AppendLine($"    Script : {TrimText(sentence.ScriptText)}");
                }

                builder.AppendLine();
            }
        }

        var paragraphOrdered = paragraphs
            .OrderByDescending(p => p.Metrics.Wer)
            .ThenByDescending(p => p.Metrics.Coverage);
        
        IEnumerable<ParagraphView> paragraphBucket;
        
        if (includeAllFlagged && hydrated?.Paragraphs is not null)
        {
            // Build a set of flagged sentence IDs
            var flaggedSentenceIds = new HashSet<int>(
                sentences.Where(s => !s.Status.Equals("ok", StringComparison.OrdinalIgnoreCase))
                        .Select(s => s.Id));

            // Build a set of paragraph IDs that contain flagged sentences
            var paragraphsWithFlaggedSentences = new HashSet<int>();
            foreach (var hydratedPara in hydrated.Paragraphs)
            {
                if (hydratedPara.SentenceIds.Any(sid => flaggedSentenceIds.Contains(sid)))
                {
                    paragraphsWithFlaggedSentences.Add(hydratedPara.Id);
                }
            }

            // Include paragraphs that are either flagged OR contain flagged sentences
            paragraphBucket = allErrors 
                ? paragraphOrdered.Where(p => !p.Status.Equals("ok", StringComparison.OrdinalIgnoreCase) || paragraphsWithFlaggedSentences.Contains(p.Id))
                : paragraphOrdered.Take(topParagraphs);
        }
        else
        {
            paragraphBucket = allErrors 
                ? paragraphOrdered.Where(p => !p.Status.Equals("ok", StringComparison.OrdinalIgnoreCase)) 
                : paragraphOrdered.Take(topParagraphs);
        }

        if (topParagraphs > 0 && paragraphs.Count > 0)
        {
            if (allErrors) builder.AppendLine("All paragraphs by WER:");
            else builder.AppendLine($"Top {Math.Min(topParagraphs, paragraphs.Count)} paragraphs by WER:");

            foreach (var paragraph in paragraphBucket)
            {
                builder.AppendLine($"  #{paragraph.Id} | WER {paragraph.Metrics.Wer:P1} | Coverage {paragraph.Metrics.Coverage:P1} | Status {paragraph.Status}");
                builder.AppendLine($"    Book range: {paragraph.BookRange.Start}-{paragraph.BookRange.End}");

                if (!string.IsNullOrWhiteSpace(paragraph.BookText))
                {
                    builder.AppendLine($"    Book   : {TrimText(paragraph.BookText)}");
                }

                builder.AppendLine();
            }
        }

        return builder.ToString().TrimEnd();
    }

    private static FileInfo ResolveAdjustmentsPath(FileInfo txFile, FileInfo? overrideFile)
    {
        if (overrideFile is not null)
        {
            return overrideFile;
        }

        string stem = GetBaseStem(txFile.Name);
        var directory = txFile.DirectoryName ?? Environment.CurrentDirectory;
        return new FileInfo(Path.Combine(directory, stem + ".pause-adjustments.json"));
    }

    private static string GetBaseStem(string fileName)
    {
        var first = Path.GetFileNameWithoutExtension(fileName);
        if (string.IsNullOrWhiteSpace(first))
        {
            return "chapter";
        }

        var second = Path.GetFileNameWithoutExtension(first);
        var stem = string.IsNullOrWhiteSpace(second) ? first : second;
        stem = NormalizeStem(stem);
        if (stem.EndsWith(".align", StringComparison.OrdinalIgnoreCase))
        {
            stem = stem[..^".align".Length];
        }

        return string.IsNullOrWhiteSpace(stem) ? "chapter" : stem;
    }

    private static string NormalizeStem(string? stem)
    {
        if (string.IsNullOrWhiteSpace(stem))
        {
            return string.Empty;
        }

        var result = stem;
        if (result.EndsWith(".treated", StringComparison.OrdinalIgnoreCase))
        {
            result = result[..^".treated".Length];
        }
        if (result.EndsWith(".pause-adjusted", StringComparison.OrdinalIgnoreCase))
        {
            result = result[..^".pause-adjusted".Length];
        }
        if (result.EndsWith(".align", StringComparison.OrdinalIgnoreCase))
        {
            result = result[..^".align".Length];
        }

        return result;
    }

    private static FileInfo? TryResolveAdjustedArtifact(FileInfo reference, string suffix)
    {
        var candidate = BuildOutputJsonPath(reference, suffix);
        return candidate.Exists ? candidate : null;
    }

    private static T LoadJson<T>(FileInfo file)
    {
        if (file is null) throw new ArgumentNullException(nameof(file));
        if (!file.Exists) throw new FileNotFoundException($"File not found: {file.FullName}", file.FullName);

        var json = File.ReadAllText(file.FullName);
        var payload = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return payload ?? throw new InvalidOperationException($"Failed to deserialize {typeof(T).Name} from {file.FullName}");
    }

    private static FileInfo? TryResolveBookIndex(string? bookIndexPath, string? fallbackDirectory)
    {
        if (string.IsNullOrWhiteSpace(bookIndexPath))
        {
            return null;
        }

        var absolute = MakeAbsolute(bookIndexPath, fallbackDirectory);
        return new FileInfo(absolute);
    }

    private static string ResolveAudioPath(TranscriptIndex transcript, HydratedTranscript hydrated, FileInfo txFile, FileInfo hydrateFile)
    {
        var directories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var stems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var existingOriginals = new List<string>();
        string? firstAbsolute = null;
        string? treatedHit = null;

        void Register(string? path, string? baseDir)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var absolute = MakeAbsolute(path, baseDir ?? txFile.DirectoryName);
            if (string.IsNullOrWhiteSpace(absolute))
            {
                return;
            }

            if (firstAbsolute is null)
            {
                firstAbsolute = absolute;
            }

            var directory = Path.GetDirectoryName(absolute);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                directories.Add(Path.GetFullPath(directory));
            }

            var stem = NormalizeStem(Path.GetFileNameWithoutExtension(absolute));
            if (!string.IsNullOrWhiteSpace(stem))
            {
                stems.Add(stem);
            }

            if (File.Exists(absolute))
            {
                existingOriginals.Add(absolute);
                if (absolute.EndsWith(".treated.wav", StringComparison.OrdinalIgnoreCase))
                {
                    treatedHit ??= absolute;
                }
            }
        }

        Register(hydrated.AudioPath, hydrateFile.DirectoryName);
        if (treatedHit is not null) return treatedHit;
        Register(transcript.AudioPath, txFile.DirectoryName);
        if (treatedHit is not null) return treatedHit;

        if (txFile.DirectoryName is not null)
        {
            directories.Add(Path.GetFullPath(txFile.DirectoryName));
            var stem = NormalizeStem(GetBaseStem(txFile.Name));
            if (!string.IsNullOrWhiteSpace(stem)) stems.Add(stem);
        }

        if (hydrateFile.DirectoryName is not null)
        {
            directories.Add(Path.GetFullPath(hydrateFile.DirectoryName));
            var stem = NormalizeStem(GetBaseStem(hydrateFile.Name));
            if (!string.IsNullOrWhiteSpace(stem)) stems.Add(stem);
        }

        foreach (var dir in directories)
        {
            foreach (var stem in stems)
            {
                if (string.IsNullOrWhiteSpace(stem)) continue;
                var treatedCandidate = Path.Combine(dir, stem + ".treated.wav");
                if (File.Exists(treatedCandidate))
                {
                    return treatedCandidate;
                }
            }
        }

        if (treatedHit is not null && File.Exists(treatedHit))
        {
            return treatedHit;
        }

        if (existingOriginals.Count > 0)
        {
            return existingOriginals[0];
        }

        if (firstAbsolute is not null)
        {
            return firstAbsolute;
        }

        throw new InvalidOperationException("Transcript does not reference an audioPath in hydrate or transcript JSON.");
    }

    private static string ResolveRoomtonePath(
        FileInfo? overrideRoomtone,
        FileInfo txFile,
        TranscriptIndex transcript,
        HydratedTranscript hydrated,
        string audioPath,
        string? scriptPath,
        string? baseDirectory,
        BookIndex? bookIndex,
        out double? stageFadeMs,
        out double? stageAppliedGainDb)
    {
        stageFadeMs = null;
        stageAppliedGainDb = null;

        if (overrideRoomtone is not null)
        {
            if (!overrideRoomtone.Exists)
            {
                throw new FileNotFoundException("Specified roomtone file not found", overrideRoomtone.FullName);
            }

            return overrideRoomtone.FullName;
        }

        var directories = new List<string>();
        var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AddDirectory(string? candidate)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                return;
            }

            string full = Path.GetFullPath(candidate);
            if (unique.Add(full) && Directory.Exists(full))
            {
                directories.Add(full);
            }
        }

        // Load stage parameters if available
        var paramsPath = Path.Combine(Path.GetDirectoryName(audioPath) ?? string.Empty, "params.snapshot.json");
        if (File.Exists(paramsPath))
        {
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(paramsPath));
                if (doc.RootElement.TryGetProperty("parameters", out var parameters))
                {
                    if (parameters.TryGetProperty("fadeMs", out var fadeElement) && fadeElement.ValueKind == JsonValueKind.Number)
                    {
                        stageFadeMs = fadeElement.GetDouble();
                    }
                    if (parameters.TryGetProperty("appliedGainDb", out var appliedGainElement) && appliedGainElement.ValueKind == JsonValueKind.Number)
                    {
                        stageAppliedGainDb = appliedGainElement.GetDouble();
                    }

                    if (parameters.TryGetProperty("roomtoneSeedPath", out var seedElement) && seedElement.ValueKind == JsonValueKind.String)
                    {
                        var seedPath = seedElement.GetString();
                        if (!string.IsNullOrWhiteSpace(seedPath) && File.Exists(seedPath))
                        {
                            return Path.GetFullPath(seedPath);
                        }
                    }
                }
            }
            catch
            {
                // Ignore and fall back to directory search
            }
        }

        AddDirectory(Path.GetDirectoryName(audioPath));

        if (!string.IsNullOrWhiteSpace(scriptPath))
        {
            var scriptFull = MakeAbsolute(scriptPath, baseDirectory);
            AddDirectory(Path.GetDirectoryName(scriptFull));
        }

        if (!string.IsNullOrWhiteSpace(baseDirectory))
        {
            AddDirectory(Path.GetFullPath(baseDirectory));
        }

        if (bookIndex?.SourceFile is not null)
        {
            var sourceFull = MakeAbsolute(bookIndex.SourceFile, baseDirectory);
            AddDirectory(Path.GetDirectoryName(sourceFull));
        }

        AddDirectory(txFile.DirectoryName);

        var current = txFile.DirectoryName;
        for (int i = 0; i < 4 && !string.IsNullOrEmpty(current); i++)
        {
            current = Path.GetDirectoryName(current);
            if (!string.IsNullOrEmpty(current))
            {
                AddDirectory(current);
            }
        }

        var candidateNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "roomtone"
        };
        void AddCandidateName(string? value)
        {
            var stem = NormalizeStem(value);
            if (!string.IsNullOrWhiteSpace(stem)) candidateNames.Add(stem);
        }

        AddCandidateName(GetBaseStem(txFile.Name));
        AddCandidateName(Path.GetFileNameWithoutExtension(audioPath));
        AddCandidateName(Path.GetFileNameWithoutExtension(transcript.AudioPath));
        AddCandidateName(Path.GetFileNameWithoutExtension(hydrated.AudioPath ?? string.Empty));

        foreach (var dir in directories)
        {
            var match = Directory.EnumerateFiles(dir, "roomtone.wav", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(path => string.Equals(Path.GetFileName(path), "roomtone.wav", StringComparison.OrdinalIgnoreCase));
            if (match is not null)
            {
                return match;
            }
        }

        foreach (var dir in directories)
        {
            var candidates = Directory.EnumerateFiles(dir, "*.wav", SearchOption.TopDirectoryOnly)
                .Where(path => !path.EndsWith(".treated.wav", StringComparison.OrdinalIgnoreCase)
                               && !path.Contains(".pause", StringComparison.OrdinalIgnoreCase)
                               && !path.Contains(".adjusted", StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => ScoreCandidate(path, candidateNames))
                .ThenBy(Path.GetFileName)
                .ToList();

            if (candidates.Count > 0)
            {
                return candidates[0];
            }
        }

        throw new FileNotFoundException("roomtone.wav not found in expected directories.");

        static int ScoreCandidate(string path, HashSet<string> names)
        {
            var stem = NormalizeStem(Path.GetFileNameWithoutExtension(path));
            if (string.Equals(stem, "roomtone", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (names.Contains(stem))
            {
                return 1;
            }

            if (stem.Contains("room", StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            return 3;
        }
    }

    private static FileInfo BuildSiblingFile(string referencePath, string suffix)
    {
        var directory = Path.GetDirectoryName(referencePath) ?? Environment.CurrentDirectory;
        var stem = Path.GetFileNameWithoutExtension(referencePath);
        return new FileInfo(Path.Combine(directory, stem + suffix));
    }

    private static void EnsureWritable(FileInfo file, bool overwrite)
    {
        if (file.DirectoryName is not null)
        {
            Directory.CreateDirectory(Path.GetFullPath(file.DirectoryName));
        }

        if (file.Exists && !overwrite)
        {
            throw new IOException($"File already exists: {file.FullName}. Use --overwrite to replace it.");
        }
    }

    private static Dictionary<int, SentenceTiming> BuildBaselineTimings(TranscriptIndex transcript, HydratedTranscript hydrated)
    {
        var map = new Dictionary<int, SentenceTiming>();

        if (hydrated.Sentences is not null)
        {
            foreach (var sentence in hydrated.Sentences)
            {
                if (sentence.Timing is { } timing && timing.Duration > 0)
                {
                    map[sentence.Id] = new SentenceTiming(timing.StartSec, timing.EndSec);
                }
            }
        }

        foreach (var sentence in transcript.Sentences)
        {
            if (!map.ContainsKey(sentence.Id))
            {
                map[sentence.Id] = new SentenceTiming(sentence.Timing.StartSec, sentence.Timing.EndSec);
            }
        }

        return map;
    }

    private static TranscriptIndex UpdateTranscriptTimings(TranscriptIndex transcript, IReadOnlyDictionary<int, SentenceTiming> timeline)
    {
        var updatedSentences = transcript.Sentences
            .Select(sentence => timeline.TryGetValue(sentence.Id, out var timing)
                ? sentence with { Timing = new TimingRange(timing.StartSec, timing.EndSec) }
                : sentence)
            .ToList();

        return transcript with { Sentences = updatedSentences };
    }

    private static HydratedTranscript UpdateHydratedTimings(HydratedTranscript hydrated, IReadOnlyDictionary<int, SentenceTiming> timeline)
    {
        var updatedSentences = hydrated.Sentences
            .Select(sentence => timeline.TryGetValue(sentence.Id, out var timing)
                ? sentence with { Timing = new TimingRange(timing.StartSec, timing.EndSec) }
                : sentence)
            .ToList();

        return hydrated with { Sentences = updatedSentences };
    }

    private static FileInfo BuildOutputJsonPath(FileInfo reference, string suffix)
    {
        string stem = GetBaseStem(reference.Name);
        var directory = reference.DirectoryName ?? Environment.CurrentDirectory;
        return new FileInfo(Path.Combine(directory, stem + suffix));
    }

    private static void SaveJson<T>(FileInfo destination, T payload)
    {
        if (destination.DirectoryName is not null)
        {
            Directory.CreateDirectory(Path.GetFullPath(destination.DirectoryName));
        }

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(destination.FullName, json);
    }

    private static double ComputeToneGainLinear(double seedMeanRmsDb, double targetRmsDb)
    {
        double seedLinear = DbToLinear(seedMeanRmsDb);
        double targetLinear = DbToLinear(targetRmsDb);
        if (seedLinear <= 0)
        {
            return 1.0;
        }

        return targetLinear / seedLinear;
    }

    private static double DbToLinear(double db) => Math.Pow(10.0, db / 20.0);

    private static string MakeAbsolute(string path, string? baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        if (Path.IsPathRooted(path))
        {
            return Path.GetFullPath(path);
        }

        var root = baseDirectory ?? Environment.CurrentDirectory;
        return Path.GetFullPath(Path.Combine(root, path));
    }

    private static readonly FrameBreathDetectorOptions BreathGuardOptions = new()
    {
        FrameMs = 25,
        HopMs = 10,
        HiSplitHz = 4000.0,
        ScoreHigh = 0.45,
        ScoreLow = 0.25,
        MinRunMs = 60,
        MergeGapMs = 40,
        GuardLeftMs = 12,
        GuardRightMs = 12,
        FricativeGuardMs = 15,
        ApplyEnergyGate = true
    };

    private const double BreathGuardMinSpanSec = 0.03;
    private const double BreathGuardSpeechThresholdDb = -34.0;

    private static IReadOnlyList<PauseAdjust> VetPauseAdjustments(
        IReadOnlyList<PauseAdjust> plannedAdjustments,
        TranscriptIndex transcript,
        AudioBuffer audio)
    {
        if (plannedAdjustments is null || plannedAdjustments.Count == 0)
        {
            return plannedAdjustments ?? Array.Empty<PauseAdjust>();
        }

        if (audio is null || audio.SampleRate <= 0 || audio.Length == 0)
        {
            return plannedAdjustments;
        }

        double audioDurationSec = audio.Length / (double)audio.SampleRate;
        if (audioDurationSec <= 0)
        {
            return plannedAdjustments;
        }

        var sentenceLookup = transcript?.Sentences?
            .Where(s => s is not null)
            .ToDictionary(s => s.Id) ?? new Dictionary<int, SentenceAlign>();

        var analyzer = new AudioAnalysisService(audio);

        var accepted = new List<PauseAdjust>(plannedAdjustments.Count);
        var rejected = new List<(PauseAdjust Adjust, double Start, double End, double RmsDb)>();

        foreach (var adjust in plannedAdjustments)
        {
            if (!ShouldVet(adjust, sentenceLookup))
            {
                accepted.Add(adjust);
                continue;
            }

            double spanStart = Math.Clamp(adjust.StartSec, 0.0, audioDurationSec);
            double spanEnd = Math.Clamp(adjust.EndSec, spanStart, audioDurationSec);
            if (spanEnd - spanStart < BreathGuardMinSpanSec)
            {
                accepted.Add(adjust);
                continue;
            }

            if (IsBreathSafe(audio, analyzer, spanStart, spanEnd))
            {
                accepted.Add(adjust);
            }
            else
            {
                double rms = analyzer.MeasureRms(spanStart, spanEnd);
                rejected.Add((adjust, spanStart, spanEnd, rms));
            }
        }

        if (rejected.Count > 0)
        {
            foreach (var (adjust, start, end, rms) in rejected)
            {
                Log.Debug(
                    "Breath guard rejected intra-sentence adjustment for sentence {SentenceId}: span=[{Start:F3},{End:F3}] original={Original:F3}s target={Target:F3}s rms={Rms:F2} dB",
                    adjust.LeftSentenceId,
                    start,
                    end,
                    adjust.OriginalDurationSec,
                    adjust.TargetDurationSec,
                    rms);
            }

            Log.Debug(
                "Breath guard removed {Rejected} of {Total} planned adjustment(s).",
                rejected.Count,
                plannedAdjustments.Count);
        }

        return accepted;
    }

    private static bool ShouldVet(PauseAdjust adjust, IReadOnlyDictionary<int, SentenceAlign> sentences)
    {
        if (adjust is null || !adjust.IsIntraSentence)
        {
            return false;
        }

        if (adjust.TargetDurationSec >= adjust.OriginalDurationSec)
        {
            return false;
        }

        if (!sentences.TryGetValue(adjust.LeftSentenceId, out var sentence))
        {
            return false;
        }

        return string.Equals(sentence.Status, "ok", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBreathSafe(AudioBuffer audio, AudioAnalysisService analyzer, double startSec, double endSec)
    {
        if (endSec - startSec <= 0)
        {
            return true;
        }

        var regions = FeatureExtraction.Detect(audio, startSec, endSec, BreathGuardOptions)
            .OrderBy(region => region.StartSec)
            .Select(region => new Region(
                Math.Clamp(region.StartSec, startSec, endSec),
                Math.Clamp(region.EndSec, startSec, endSec)))
            .Where(region => region.DurationSec > 0)
            .ToList();

        if (regions.Count == 0)
        {
            double rms = analyzer.MeasureRms(startSec, endSec);
            return rms <= BreathGuardSpeechThresholdDb;
        }

        double cursor = startSec;
        foreach (var region in regions)
        {
            double gapStart = cursor;
            double gapEnd = region.StartSec;
            if (gapEnd - gapStart >= BreathGuardMinSpanSec)
            {
                double rms = analyzer.MeasureRms(gapStart, gapEnd);
                if (rms > BreathGuardSpeechThresholdDb)
                {
                    return false;
                }
            }

            cursor = Math.Max(cursor, region.EndSec);
        }

        if (endSec - cursor >= BreathGuardMinSpanSec)
        {
            double rms = analyzer.MeasureRms(cursor, endSec);
            if (rms > BreathGuardSpeechThresholdDb)
            {
                return false;
            }
        }

        return true;
    }

    private static string TrimText(string text, int? maxLength = null)
    {
        var normalized = text.Replace('\n', ' ').Replace('\r', ' ').Trim();
        if (maxLength is null || normalized.Length <= maxLength.Value)
        {
            return normalized;
        }

        return normalized[..maxLength.Value].TrimEnd() + "…";
    }

    private sealed record SourceInfo(string AudioPath, string ScriptPath, string BookIndexPath, DateTime CreatedAtUtc);

    private sealed record SentenceView(
        int Id,
        (int Start, int End) BookRange,
        (int? Start, int? End)? ScriptRange,
        SentenceMetrics Metrics,
        string Status,
        string? BookText,
        string? ScriptText,
        TimingRange? Timing);

    private sealed record ParagraphView(
        int Id,
        (int Start, int End) BookRange,
        ParagraphMetrics Metrics,
        string Status,
        string? BookText);

    private sealed record WordTallies(int Match, int Substitution, int Insertion, int Deletion, int Total);

    private sealed record ReportResult(
        string Report,
        IReadOnlyList<SentenceView> Sentences,
        IReadOnlyList<ParagraphView> Paragraphs,
        WordTallies? WordTallies);

}
