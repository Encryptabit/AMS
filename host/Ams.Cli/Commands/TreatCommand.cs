using System.CommandLine;
using Ams.Cli.Repl;
using Ams.Cli.Utilities;
using Ams.Core.Audio;
using Ams.Core.Runtime.Workspace;

namespace Ams.Cli.Commands;

/// <summary>
/// CLI command for audio treatment (assembling production-ready chapter audio with roomtone).
/// </summary>
public static class TreatCommand
{
    public static Command Create()
    {
        var cmd = new Command("treat", "Treat chapter audio with roomtone padding (preroll, gap, postroll)");

        var roomtoneOption = new Option<FileInfo?>(
            "--roomtone",
            "Override roomtone.wav file (defaults to book's roomtone.wav via Book.Audio.Roomtone)");
        roomtoneOption.AddAlias("-r");

        var prerollOption = new Option<double?>(
            "--preroll",
            "Preroll duration in seconds (default: 0.75)");

        var gapOption = new Option<double?>(
            "--gap",
            "Chapter-to-content gap duration in seconds (default: 1.5)");

        var postrollOption = new Option<double?>(
            "--postroll",
            "Postroll duration in seconds (default: 3.0)");

        var silenceThresholdOption = new Option<double?>(
            "--silence-threshold-db",
            "Silence detection threshold in dB (default: -55; quieter speech may need around -63)");
        silenceThresholdOption.AddAlias("--silence-db");

        var minSilenceDurationOption = new Option<double?>(
            "--min-silence-duration",
            "Minimum silence duration in seconds (default: 0.05)");
        minSilenceDurationOption.AddAlias("--min-silence");

        var forceOption = new Option<bool>(
            "--force",
            () => false,
            "Overwrite existing treated.wav files");
        forceOption.AddAlias("-f");

        cmd.AddOption(roomtoneOption);
        cmd.AddOption(prerollOption);
        cmd.AddOption(gapOption);
        cmd.AddOption(postrollOption);
        cmd.AddOption(silenceThresholdOption);
        cmd.AddOption(minSilenceDurationOption);
        cmd.AddOption(forceOption);

        cmd.SetHandler(async context =>
        {
            var cancellationToken = context.GetCancellationToken();

            try
            {
                var roomtoneValue = context.ParseResult.GetValueForOption(roomtoneOption);
                var prerollValue = context.ParseResult.GetValueForOption(prerollOption);
                var gapValue = context.ParseResult.GetValueForOption(gapOption);
                var postrollValue = context.ParseResult.GetValueForOption(postrollOption);
                var silenceThresholdValue = context.ParseResult.GetValueForOption(silenceThresholdOption);
                var minSilenceDurationValue = context.ParseResult.GetValueForOption(minSilenceDurationOption);
                var force = context.ParseResult.GetValueForOption(forceOption);

                // Get active chapter from REPL context
                var repl = ReplContext.Current;
                if (repl?.ActiveChapter is null)
                {
                    Log.Error("No active chapter. Use 'use <chapter>' to select one or 'mode all' for batch.");
                    context.ExitCode = 1;
                    return;
                }

                var activeChapter = repl.ActiveChapter;
                var chapterId = Path.GetFileNameWithoutExtension(activeChapter.Name);

                // Resolve book index and workspace
                var bookIndexFile = CommandInputResolver.ResolveBookIndex(null, mustExist: true);
                var workspace = CommandInputResolver.ResolveWorkspace(bookIndexFile);

                // Build treatment options
                var options = new TreatmentOptions();
                if (prerollValue.HasValue)
                {
                    options = options with { PrerollSeconds = prerollValue.Value };
                }
                if (gapValue.HasValue)
                {
                    options = options with { ChapterToContentGapSeconds = gapValue.Value };
                }
                if (postrollValue.HasValue)
                {
                    options = options with { PostrollSeconds = postrollValue.Value };
                }
                if (silenceThresholdValue.HasValue)
                {
                    if (silenceThresholdValue.Value > 0 || silenceThresholdValue.Value < -120)
                    {
                        Log.Error("Invalid --silence-threshold-db value {Value}. Expected a range between -120 and 0 dB.", silenceThresholdValue.Value);
                        context.ExitCode = 1;
                        return;
                    }

                    options = options with { SilenceThresholdDb = silenceThresholdValue.Value };
                }
                if (minSilenceDurationValue.HasValue)
                {
                    if (minSilenceDurationValue.Value <= 0 || minSilenceDurationValue.Value > 10)
                    {
                        Log.Error("Invalid --min-silence-duration value {Value}. Expected a range greater than 0 and up to 10 seconds.", minSilenceDurationValue.Value);
                        context.ExitCode = 1;
                        return;
                    }

                    options = options with { MinimumSilenceDuration = minSilenceDurationValue.Value };
                }

                // Open chapter context
                var openOptions = new ChapterOpenOptions
                {
                    BookIndexFile = bookIndexFile,
                    AudioFile = activeChapter,
                    ChapterId = chapterId
                };

                using var handle = workspace.OpenChapter(openOptions);
                var chapter = handle.Chapter;

                // Determine output path
                var outputPath = chapter.ResolveArtifactFile("treated.wav").FullName;

                // Check if hydrate.json exists (indicates alignment was completed)
                if (chapter.Documents.HydratedTranscript is null)
                {
                    var hydrateFile = chapter.ResolveArtifactFile("align.hydrate.json");
                    if (!hydrateFile.Exists)
                    {
                        Log.Warn("Skipping {Chapter}: hydrate.json not found (alignment not completed)", chapterId);
                        context.ExitCode = 0; // Not an error, just skip
                        return;
                    }
                }

                // Check if output already exists
                if (File.Exists(outputPath) && !force)
                {
                    Log.Debug("Skipping {Chapter}: treated.wav already exists (use --force to overwrite)", chapterId);
                    context.ExitCode = 0;
                    return;
                }

                Log.Info("Treating chapter: {Chapter}", chapterId);

                var service = new AudioTreatmentService();
                AudioTreatmentService.TreatmentResult result;

                if (roomtoneValue is not null)
                {
                    // Use explicit roomtone override
                    if (!roomtoneValue.Exists)
                    {
                        Log.Error("Roomtone file not found: {Path}", roomtoneValue.FullName);
                        context.ExitCode = 1;
                        return;
                    }

                    Log.Debug("Using roomtone override: {Path}", roomtoneValue.FullName);
                    result = await service.TreatChapterAsync(
                        chapter,
                        roomtoneValue.FullName,
                        outputPath,
                        options,
                        cancellationToken);
                }
                else
                {
                    // Use book's roomtone via Book.Audio.Roomtone
                    if (!chapter.Book.Audio.HasRoomtone)
                    {
                        Log.Error("Roomtone file not found: {Path}", chapter.Book.Audio.RoomtonePath);
                        Log.Error("Create a roomtone.wav file in the book directory or specify with --roomtone.");
                        context.ExitCode = 1;
                        return;
                    }

                    Log.Debug("Using book roomtone: {Path}", chapter.Book.Audio.RoomtonePath);
                    result = await service.TreatChapterAsync(
                        chapter,
                        outputPath,
                        options,
                        cancellationToken);
                }

                if (result.TitleStartSec >= 0)
                {
                    Log.Info("  Title: {Start:F2}s - {End:F2}s",
                        result.TitleStartSec, result.TitleEndSec);
                }
                else
                {
                    Log.Info("  Title: (none detected)");
                }
                Log.Info("  Content: {Start:F2}s - {End:F2}s",
                    result.ContentStartSec, result.ContentEndSec);
                Log.Info("  Output: {Path} ({Duration:F1}s)",
                    result.OutputPath, result.TotalDurationSec);
            }
            catch (OperationCanceledException)
            {
                Log.Debug("Treatment cancelled");
                context.ExitCode = 1;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Treatment failed");
                context.ExitCode = 1;
            }
        });

        return cmd;
    }
}
