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
            "Path to roomtone.wav file (defaults to book directory)");
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

        var forceOption = new Option<bool>(
            "--force",
            () => false,
            "Overwrite existing treated.wav files");
        forceOption.AddAlias("-f");

        cmd.AddOption(roomtoneOption);
        cmd.AddOption(prerollOption);
        cmd.AddOption(gapOption);
        cmd.AddOption(postrollOption);
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

                // Resolve roomtone path
                var roomtonePath = ResolveRoomtonePath(roomtoneValue, workspace.RootPath);
                if (!File.Exists(roomtonePath))
                {
                    Log.Error("Roomtone file not found: {Path}", roomtonePath);
                    Log.Error("Expected roomtone.wav in book directory. Create or specify with --roomtone.");
                    context.ExitCode = 1;
                    return;
                }

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
                var result = await service.TreatChapterAsync(
                    chapter,
                    roomtonePath,
                    outputPath,
                    options,
                    cancellationToken);

                Log.Info("  Title: {Start:F2}s - {End:F2}s",
                    result.TitleStartSec, result.TitleEndSec);
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

    private static string ResolveRoomtonePath(FileInfo? provided, string workspaceRoot)
    {
        if (provided is not null)
        {
            return provided.FullName;
        }

        // Default to roomtone.wav in workspace root
        return Path.Combine(workspaceRoot, "roomtone.wav");
    }
}
