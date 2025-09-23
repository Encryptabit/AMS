using System.CommandLine;
using System.Text;
using Ams.Core.Book;

namespace Ams.Cli.Commands;

public static class PipelineCommand
{
    public static Command Create()
    {
        var pipeline = new Command("pipeline", "Run the end-to-end chapter pipeline");
        pipeline.AddCommand(CreateRun());
        return pipeline;
    }

    private static Command CreateRun()
    {
        var cmd = new Command("run", "Build index, run ASR, align, hydrate, and apply roomtone in one pass");

        var bookOption = new Option<FileInfo>("--book", "Path to the book manuscript (DOCX/TXT/etc.)") { IsRequired = true };
        bookOption.AddAlias("-b");

        var audioOption = new Option<FileInfo>("--audio", "Path to the chapter audio WAV") { IsRequired = true };
        audioOption.AddAlias("-a");

        var workDirOption = new Option<DirectoryInfo?>("--work-dir", () => null, "Working directory for generated artifacts");
        var bookIndexOption = new Option<FileInfo?>("--book-index", () => null, "Existing/target BookIndex JSON path (defaults to work-dir/book-index.json)");
        var chapterIdOption = new Option<string?>("--chapter-id", () => null, "Override output stem (defaults to audio file name)");
        var forceIndexOption = new Option<bool>("--force-index", () => false, "Rebuild book index even if it already exists");
        var avgWpmOption = new Option<double>("--avg-wpm", () => 200.0, "Average WPM used for duration estimation when indexing");

        var asrServiceOption = new Option<string>("--asr-service", () => "http://localhost:8000", "ASR service URL");
        asrServiceOption.AddAlias("-s");
        var asrModelOption = new Option<string?>("--asr-model", () => null, "Optional ASR model identifier");
        asrModelOption.AddAlias("-m");
        var sampleRateOption = new Option<int>("--sample-rate", () => 44100, "Roomtone output sample rate (Hz)");
        var bitDepthOption = new Option<int>("--bit-depth", () => 32, "Roomtone output bit depth");
        var fadeMsOption = new Option<double>("--fade-ms", () => 10.0, "Crossfade length for roomtone boundaries (ms)");
        var toneDbOption = new Option<double>("--tone-gain-db", () => -60.0, "Target RMS level for roomtone (dBFS)");
        var diagnosticsOption = new Option<bool>("--emit-diagnostics", () => false, "Emit diagnostic WAVs during roomtone rendering");
        var adaptiveGainOption = new Option<bool>("--adaptive-gain", () => false, "Scale roomtone seed to match target RMS");

        cmd.AddOption(bookOption);
        cmd.AddOption(audioOption);
        cmd.AddOption(workDirOption);
        cmd.AddOption(bookIndexOption);
        cmd.AddOption(chapterIdOption);
        cmd.AddOption(forceIndexOption);
        cmd.AddOption(avgWpmOption);
        cmd.AddOption(asrServiceOption);
        cmd.AddOption(asrModelOption);
        cmd.AddOption(sampleRateOption);
        cmd.AddOption(bitDepthOption);
        cmd.AddOption(fadeMsOption);
        cmd.AddOption(toneDbOption);
        cmd.AddOption(diagnosticsOption);
        cmd.AddOption(adaptiveGainOption);

        cmd.SetHandler(async context =>
        {
            var bookFile = context.ParseResult.GetValueForOption(bookOption)!;
            var audioFile = context.ParseResult.GetValueForOption(audioOption)!;
            var workDir = context.ParseResult.GetValueForOption(workDirOption);
            var bookIndex = context.ParseResult.GetValueForOption(bookIndexOption);
            var chapterId = context.ParseResult.GetValueForOption(chapterIdOption);
            var forceIndex = context.ParseResult.GetValueForOption(forceIndexOption);
            var avgWpm = context.ParseResult.GetValueForOption(avgWpmOption);
            var asrService = context.ParseResult.GetValueForOption(asrServiceOption) ?? "http://localhost:8000";
            var asrModel = context.ParseResult.GetValueForOption(asrModelOption);
            var sampleRate = context.ParseResult.GetValueForOption(sampleRateOption);
            var bitDepth = context.ParseResult.GetValueForOption(bitDepthOption);
            var fadeMs = context.ParseResult.GetValueForOption(fadeMsOption);
            var toneDb = context.ParseResult.GetValueForOption(toneDbOption);
            var emitDiagnostics = context.ParseResult.GetValueForOption(diagnosticsOption);
            var adaptiveGain = context.ParseResult.GetValueForOption(adaptiveGainOption);

            try
            {
                await RunPipelineAsync(
                    bookFile,
                    audioFile,
                    workDir,
                    bookIndex,
                    chapterId,
                    forceIndex,
                    avgWpm,
                    asrService,
                    asrModel,
                    sampleRate,
                    bitDepth,
                    fadeMs,
                    toneDb,
                    emitDiagnostics,
                    adaptiveGain);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        });

        return cmd;
    }

    private static async Task RunPipelineAsync(
        FileInfo bookFile,
        FileInfo audioFile,
        DirectoryInfo? workDirOption,
        FileInfo? bookIndexOverride,
        string? chapterIdOverride,
        bool forceIndex,
        double avgWpm,
        string asrService,
        string? asrModel,
        int sampleRate,
        int bitDepth,
        double fadeMs,
        double toneDb,
        bool emitDiagnostics,
        bool adaptiveGain)
    {
        if (!bookFile.Exists)
        {
            throw new FileNotFoundException($"Book file not found: {bookFile.FullName}");
        }

        if (!audioFile.Exists)
        {
            throw new FileNotFoundException($"Audio file not found: {audioFile.FullName}");
        }

        var workDirPath = workDirOption?.FullName ?? audioFile.Directory?.FullName ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(workDirPath);

        var chapterStem = MakeSafeFileStem(string.IsNullOrWhiteSpace(chapterIdOverride)
            ? Path.GetFileNameWithoutExtension(audioFile.Name)
            : chapterIdOverride!);
        var chapterDir = Path.Combine(workDirPath, chapterStem);
        EnsureDirectory(chapterDir);

        var bookIndexFile = bookIndexOverride ?? new FileInfo(Path.Combine(workDirPath, "book-index.json"));
        EnsureDirectory(bookIndexFile.DirectoryName);

        var asrFile = new FileInfo(Path.Combine(chapterDir, $"{chapterStem}.asr.json"));
        var anchorsFile = new FileInfo(Path.Combine(chapterDir, $"{chapterStem}.align.anchors.json"));
        var txFile = new FileInfo(Path.Combine(chapterDir, $"{chapterStem}.align.tx.json"));
        var hydrateFile = new FileInfo(Path.Combine(chapterDir, $"{chapterStem}.align.hydrate.json"));
        var treatedWav = new FileInfo(Path.Combine(chapterDir, $"{chapterStem}.treated.wav"));

        Console.WriteLine("=== AMS Pipeline ===");
        Console.WriteLine($"Book:    {bookFile.FullName}");
        Console.WriteLine($"Audio:   {audioFile.FullName}");
        Console.WriteLine($"WorkDir: {workDirPath}");
        Console.WriteLine($"Chapter: {chapterStem}");
        Console.WriteLine($"ChapterDir: {chapterDir}");
        Console.WriteLine();

        if (forceIndex || !bookIndexFile.Exists)
        {
            Console.WriteLine(forceIndex ? "Rebuilding book index..." : "Building book index...");
            await BuildIndexCommand.BuildBookIndexAsync(
                bookFile,
                bookIndexFile,
                forceIndex,
                new BookIndexOptions { AverageWpm = avgWpm },
                noCache: false);
        }
        else
        {
            Console.WriteLine($"Book index already present at {bookIndexFile.FullName}; skipping (use --force-index to rebuild).");
        }

        if (!bookIndexFile.Exists)
        {
            throw new InvalidOperationException($"Book index file missing after build: {bookIndexFile.FullName}");
        }

        Console.WriteLine("\nRunning ASR...");
        EnsureDirectory(asrFile.DirectoryName);
        await AsrCommand.RunAsrAsync(audioFile, asrFile, asrService, asrModel);

        Console.WriteLine("Computing anchors...");
        await AlignCommand.RunAnchorsAsync(
            bookIndexFile,
            asrFile,
            anchorsFile,
            detectSection: true,
            ngram: 3,
            targetPerTokens: 50,
            minSeparation: 100,
            crossSentences: false,
            domainStopwords: true,
            asrPrefixTokens: 8,
            emitWindows: false);

        Console.WriteLine("Generating transcript index...");
        await AlignCommand.RunTranscriptIndexAsync(
            bookIndexFile,
            asrFile,
            audioFile,
            txFile,
            detectSection: true,
            asrPrefixTokens: 8,
            ngram: 3,
            targetPerTokens: 50,
            minSeparation: 100,
            crossSentences: false,
            domainStopwords: true);

        Console.WriteLine("Hydrating transcript...");
        await AlignCommand.RunHydrateTxAsync(bookIndexFile, asrFile, txFile, hydrateFile);

        Console.WriteLine("Rendering roomtone...");
        await AudioCommand.RunRenderAsync(txFile, treatedWav, sampleRate, bitDepth, fadeMs, toneDb, emitDiagnostics, adaptiveGain);

        Console.WriteLine();
        Console.WriteLine("=== Outputs ===");
        Console.WriteLine($"Book index : {bookIndexFile.FullName}");
        Console.WriteLine($"ASR JSON   : {asrFile.FullName}");
        Console.WriteLine($"Anchors    : {anchorsFile.FullName}");
        Console.WriteLine($"Transcript : {txFile.FullName}");
        Console.WriteLine($"Hydrated   : {hydrateFile.FullName}");
        Console.WriteLine($"Roomtone   : {treatedWav.FullName}");
    }

    private static void EnsureDirectory(string? dir)
    {
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    private static string MakeSafeFileStem(string? value)
    {
        const string fallback = "chapter";
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var invalid = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            builder.Append(Array.IndexOf(invalid, ch) >= 0 ? '_' : ch);
        }

        var result = builder.ToString().Trim();
        return string.IsNullOrEmpty(result) ? fallback : result;
    }
}
