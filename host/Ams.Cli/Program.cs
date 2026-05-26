using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Text;
using System.Threading;
using Ams.Cli.Repl;
using Ams.Cli.Commands;
using Ams.Core.Application.Benchmark;
using Ams.Core.Application.Mfa;
using Ams.Core.Application.Validation;
using Ams.Core.Asr;
using Ams.Core.Runtime.Audio;
using Ams.Core.Runtime.Book;
using Ams.Core.Services;
using Ams.Core.Services.Alignment;
using Ams.Core.Services.Documents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ams.Core.Services.Interfaces;

namespace Ams.Cli;

public static class Program
{
    private static async Task<int> Main(string[] args)
    {
        using IHost host = BuildHost(args);
        var rootCommand = BuildRootCommand(host.Services);

        using var loggerFactory = Log.ConfigureDefaults(logFileName: "ams-log.txt");
        Log.Debug("Structured logging initialized. Console + file at {LogFile}", Log.LogFilePath ?? "(unknown)");

        var configuredEngine = AsrEngineConfig.Resolve();
        Log.Debug("ASR engine configured as {Engine}", configuredEngine);
        Log.Debug("In-process ASR selected; skipping external service warmup.");

        MfaProcessSupervisor.RegisterForShutdown();
        MfaProcessSupervisor.TriggerBackgroundWarmup();

        await host.StartAsync();

        // if no args , repl by default
        if (args.Length == 0)
        {
            await StartRepl(rootCommand);
            return 0;
        }

        return await CreateParser(rootCommand).InvokeAsync(args);
    }

    public static IHost BuildHost(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        ConfigureServices(builder.Services);
        return builder.Build();
    }

    public static RootCommand BuildRootCommand(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var generateTranscript = services.GetRequiredService<GenerateTranscriptCommand>();
        var computeAnchors = services.GetRequiredService<ComputeAnchorsCommand>();
        var buildBookIndex = services.GetRequiredService<BuildBookIndexCommand>();
        var transcriptIndexCommand = services.GetRequiredService<BuildTranscriptIndexCommand>();
        var hydrateCommand = services.GetRequiredService<HydrateTranscriptCommand>();
        var pipelineService = services.GetRequiredService<PipelineService>();
        var validationService = services.GetRequiredService<ValidationService>();
        var benchmarkRunService = services.GetRequiredService<BenchmarkRunService>();
        var benchmarkCompareService = services.GetRequiredService<BenchmarkCompareService>();

        var rootCommand = new RootCommand("AMS - Audio Management System CLI");

        rootCommand.AddCommand(AsrCommand.Create(generateTranscript));
        rootCommand.AddCommand(ValidateCommand.Create(validationService));
        rootCommand.AddCommand(TextCommand.Create());
        rootCommand.AddCommand(BuildIndexCommand.Create(buildBookIndex));
        rootCommand.AddCommand(BookCommand.Create());
        rootCommand.AddCommand(AlignCommand.Create(computeAnchors, transcriptIndexCommand, hydrateCommand));
        rootCommand.AddCommand(RefineSentencesCommand.Create());
        rootCommand.AddCommand(PipelineCommand.Create(pipelineService));
        rootCommand.AddCommand(BenchmarkCommand.Create(benchmarkRunService, benchmarkCompareService));
        rootCommand.AddCommand(DspCommand.Create());
        rootCommand.AddCommand(TreatCommand.Create());
        rootCommand.AddCommand(QcCommand.Create());

        var replCommand = new Command("repl", "Start interactive REPL");
        replCommand.SetHandler(async () => await StartRepl(rootCommand));
        rootCommand.AddCommand(replCommand);

        return rootCommand;
    }

    private static Parser CreateParser(RootCommand rootCommand)
    {
        ArgumentNullException.ThrowIfNull(rootCommand);
        return new CommandLineBuilder(rootCommand).UseDefaults().Build();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IPronunciationProvider>(_ => new MfaPronunciationProvider());
        services.AddSingleton<IBookCache>(_ => new BookCache());
        services.AddSingleton<IAppPlaybackAlertSoundService, AppPlaybackAlertSoundService>();
        services.AddSingleton<IDocumentService, DocumentService>();
        services.AddSingleton<IAsrService, AsrService>();
        services.AddSingleton<IAnchorComputeService, AnchorComputeService>();
        services.AddSingleton<ITranscriptIndexService, TranscriptIndexService>();
        services.AddSingleton<ITranscriptHydrationService, TranscriptHydrationService>();
        services.AddSingleton<IAlignmentService, AlignmentService>();
        services.AddSingleton<GenerateTranscriptCommand>();
        services.AddSingleton<ComputeAnchorsCommand>();
        services.AddSingleton<BuildBookIndexCommand>();
        services.AddSingleton<BuildTranscriptIndexCommand>();
        services.AddSingleton<HydrateTranscriptCommand>();
        services.AddSingleton<RunMfaCommand>();
        services.AddSingleton<MergeTimingsCommand>();
        services.AddSingleton<PipelineService>();
        services.AddSingleton<ValidationService>();

        services.AddSingleton<IBenchmarkDependencyReadinessProbe, BenchmarkDependencyReadinessProbe>();
        services.AddSingleton<BenchmarkDeterminismGate>();
        services.AddSingleton<BenchmarkRunArtifactStore>();
        services.AddSingleton<BenchmarkRunManifestValidator>();
        services.AddSingleton<IBenchmarkMetricsCollector, BenchmarkMetricsCollector>();
        services.AddSingleton<BenchmarkRunService>();
        services.AddSingleton<BenchmarkCompareService>();
    }

    private static async Task StartRepl(RootCommand rootCommand)
    {
        // RootCommand.InvokeAsync builds the default invocation pipeline per call, and
        // that path mutates the command tree by adding default global options. The REPL
        // invokes the same root many times, sometimes concurrently for chapter batches,
        // so use one parser for the whole session.
        var parser = CreateParser(rootCommand);
        var state = new ReplState();
        var lineEditor = new ReplLineEditor();
        Console.WriteLine("AMS Interactive CLI - Type 'help' for CLI verbs, 'exit' to quit");
        Console.WriteLine("Built-ins: set-dir, list-wav, use, mode, run, state, clear");

        while (true)
        {
            var input = lineEditor.ReadLine(Prompt(state));
            if (input is null)
            {
                Console.WriteLine();
                break;
            }

            input = input.Trim();
            if (input.Length == 0)
            {
                continue;
            }

            if (IsExit(input))
            {
                Console.WriteLine("Exiting...");
                break;
            }

            if (await TryHandleBuiltInAsync(input, state, parser))
            {
                Console.WriteLine();
                continue;
            }

            try
            {
                var replArgs = ParseInput(input);
                await ExecuteWithScopeAsync(replArgs, state, parser);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine();
        }
    }

    private static string[] ParseInput(string input)
    {
        var args = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        foreach (var c in input)
        {
            switch (c)
            {
                case '"':
                    inQuotes = !inQuotes;
                    break;
                case ' ' when !inQuotes:
                    if (current.Length > 0)
                    {
                        args.Add(current.ToString());
                        current.Clear();
                    }

                    break;
                default:
                    current.Append(c);
                    break;
            }
        }

        if (current.Length > 0)
        {
            args.Add(current.ToString());
        }

        return args.ToArray();
    }

    private static string Prompt(ReplState state)
    {
        var dirLabel = state.WorkingDirectoryLabel;
        var scopeLabel = state.ScopeLabel;
        var asrLabel = GetAsrStatusLabel();
        return $"[AMS|{dirLabel}|{scopeLabel}|{asrLabel}]> ";
    }

    private static bool IsExit(string input)
    {
        return input.Equals("exit", StringComparison.OrdinalIgnoreCase)
               || input.Equals("quit", StringComparison.OrdinalIgnoreCase)
               || input.Equals("q", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetAsrStatusLabel() => $"ASR:{AsrEngineConfig.Resolve().ToString().ToLowerInvariant()}";

    private static async Task<bool> TryHandleBuiltInAsync(string input, ReplState state, Parser parser)
    {
        var tokens = ParseInput(input);
        if (tokens.Length == 0)
        {
            return true;
        }

        var head = tokens[0].ToLowerInvariant();
        switch (head)
        {
            case "help":
                await parser.InvokeAsync(new[] { "--help" });
                return true;

            case "clear":
                Console.Clear();
                state.PrintState();
                return true;

            case "state":
                state.PrintState();
                return true;

            case "set-dir":
            case "dir":
                await HandleDirectoryCommandAsync(tokens, state);
                return true;

            case "list-wav":
            case "lswav":
            case "list":
                state.ListChapters();
                return true;

            case "use":
                HandleUseCommand(tokens, state);
                return true;

            case "mode":
                HandleModeCommand(tokens, state);
                return true;

            case "run":
                if (tokens.Length < 2)
                {
                    Console.WriteLine("Usage: run <command> [args...]");
                    return true;
                }

                var runArgs = tokens.Skip(1).ToArray();
                await ExecuteWithScopeAsync(runArgs, state, parser);
                return true;

            default:
                return false;
        }
    }

    private static async Task HandleDirectoryCommandAsync(IReadOnlyList<string> tokens, ReplState state)
    {
        await Task.CompletedTask;

        if (tokens.Count == 1 || (tokens.Count == 2 && tokens[1].Equals("show", StringComparison.OrdinalIgnoreCase)))
        {
            state.PrintState();
            return;
        }

        if (tokens.Count >= 2 && tokens[1].Equals("set", StringComparison.OrdinalIgnoreCase))
        {
            if (tokens.Count < 3)
            {
                Console.WriteLine("Usage: set-dir set <path>");
                return;
            }

            var path = string.Join(' ', tokens.Skip(2));
            state.SetWorkingDirectory(path);
            state.PrintState();
            return;
        }

        if (tokens.Count >= 2)
        {
            var path = string.Join(' ', tokens.Skip(1));
            state.SetWorkingDirectory(path);
            state.PrintState();
            return;
        }
    }

    private static void HandleUseCommand(IReadOnlyList<string> tokens, ReplState state)
    {
        if (tokens.Count < 2)
        {
            Console.WriteLine("Usage: use <all|index|name>");
            return;
        }

        var value = tokens[1];
        if (value.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            state.UseAllChapters();
            state.PrintState();
            return;
        }

        if (int.TryParse(value, out var idx))
        {
            if (!state.UseChapterByIndex(idx))
            {
                Console.WriteLine("Invalid chapter index");
            }

            state.PrintState();
            return;
        }

        if (!state.UseChapterByName(string.Join(' ', tokens.Skip(1))))
        {
            Console.WriteLine("Chapter not found");
        }

        state.PrintState();
    }

    private static void HandleModeCommand(IReadOnlyList<string> tokens, ReplState state)
    {
        if (tokens.Count == 1)
        {
            Console.WriteLine($"Current mode: {state.ScopeLabel}");
            return;
        }

        HandleUseCommand(tokens, state);
    }

    private static async Task ExecuteWithScopeAsync(string[] args, ReplState state, Parser parser)
    {
        if (args.Length == 0)
        {
            return;
        }

        var originalDirectory = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(state.WorkingDirectory);

            if (state.RunAllChapters && state.Chapters.Count > 0)
            {
                if (TryGetChapterParallelism(args, out var parallelism))
                {
                    await ExecuteChaptersInParallelAsync(state, parser, args, parallelism);
                    return;
                }
                else if (ShouldHandleAllChaptersInBulk(args))
                {
                    ReplContext.Current = state;
                    try
                    {
                        await parser.InvokeAsync(args);
                    }
                    finally
                    {
                        ReplContext.Current = null;
                    }
                }
                else
                {
                    foreach (var chapter in state.Chapters)
                    {
                        Console.WriteLine($"=== {chapter.Name} ===");
                        using var scope = state.BeginChapterScope(chapter);
                        ReplContext.Current = state;
                        try
                        {
                            var concreteArgs = ReplacePlaceholders(args, chapter);
                            var exitCode = await parser.InvokeAsync(concreteArgs);
                            if (exitCode != 0)
                            {
                                Console.WriteLine($"Command exited with {exitCode}; stopping batch.");
                                break;
                            }
                        }
                        finally
                        {
                            ReplContext.Current = null;
                        }
                    }
                }
            }
            else if (state.SelectedChapter is not null)
            {
                var chapter = state.SelectedChapter;
                using var scope = state.BeginChapterScope(chapter);
                ReplContext.Current = state;
                try
                {
                    var concreteArgs = ReplacePlaceholders(args, chapter);
                    await parser.InvokeAsync(concreteArgs);
                }
                finally
                {
                    ReplContext.Current = null;
                }
            }
            else
            {
                ReplContext.Current = state;
                try
                {
                    await parser.InvokeAsync(args);
                }
                finally
                {
                    ReplContext.Current = null;
                }
            }
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    private static async Task ExecuteChaptersInParallelAsync(
        ReplState state,
        Parser parser,
        string[] args,
        int requestedParallelism)
    {
        var chapters = state.Chapters;
        if (chapters.Count == 0)
        {
            return;
        }

        var degree = Math.Clamp(requestedParallelism, 1, chapters.Count);
        var commandLabel = chapters.Count > 0 && args.Length > 0 ? args[0] : "command";
        Log.Debug("Running {Command} in parallel with degree {Degree} (requested {Requested})", commandLabel, degree, requestedParallelism);

        using var semaphore = new SemaphoreSlim(degree);
        var tasks = new List<Task>(chapters.Count);
        var failures = 0;

        foreach (var chapter in chapters)
        {
            await semaphore.WaitAsync().ConfigureAwait(false);
            var chapterCopy = chapter;
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    Console.WriteLine($"=== {chapterCopy.Name} ===");
                    using var scope = state.BeginChapterScope(chapterCopy);
                    ReplContext.Current = state;
                    var concreteArgs = ReplacePlaceholders(args, chapterCopy);
                    var exitCode = await parser.InvokeAsync(concreteArgs).ConfigureAwait(false);
                    if (exitCode != 0)
                    {
                        Interlocked.Increment(ref failures);
                    }
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref failures);
                    Console.WriteLine($"Chapter {chapterCopy.Name} failed: {ex.Message}");
                }
                finally
                {
                    ReplContext.Current = null;
                    semaphore.Release();
                }
            }));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        if (failures > 0)
        {
            Console.WriteLine($"{failures} chapter(s) reported failures during parallel execution.");
        }
    }

    private static bool TryGetChapterParallelism(IReadOnlyList<string> args, out int parallelism)
    {
        parallelism = 1;
        if (args.Count < 2)
        {
            return false;
        }

        var verb = args[0];
        var sub = args[1];

        // ASR opt-in: explicit --parallel N flag.
        if (verb.Equals("asr", StringComparison.OrdinalIgnoreCase) &&
            sub.Equals("run", StringComparison.OrdinalIgnoreCase))
        {
            parallelism = Math.Max(1, ExtractParallelism(args));
            return parallelism > 1;
        }

        // Treat: each chapter's treatment is independent and CPU-bound, so default
        // mode-all batches to ProcessorCount-wide parallelism without requiring a flag.
        if (verb.Equals("treat", StringComparison.OrdinalIgnoreCase))
        {
            parallelism = Math.Max(1, Environment.ProcessorCount);
            return parallelism > 1;
        }

        return false;
    }

    private static int ExtractParallelism(IReadOnlyList<string> args)
    {
        for (int i = 0; i < args.Count; i++)
        {
            var token = args[i];
            if (token.Equals("--parallel", StringComparison.OrdinalIgnoreCase) ||
                token.Equals("-p", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < args.Count && int.TryParse(args[i + 1], out var value))
                {
                    return NormalizeParallelism(value);
                }

                return 1;
            }

            const string prefix = "--parallel=";
            if (token.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var inline = token.Substring(prefix.Length);
                if (int.TryParse(inline, out var inlineValue))
                {
                    return NormalizeParallelism(inlineValue);
                }
            }
        }

        return 1;
    }

    private static int NormalizeParallelism(int requested)
    {
        if (requested > 0)
        {
            return requested;
        }

        var auto = Math.Max(1, Environment.ProcessorCount);
        return auto;
    }

    private static bool ShouldHandleAllChaptersInBulk(IReadOnlyList<string> args)
    {
        if (args.Count < 2)
        {
            return false;
        }

        if (!args[0].Equals("pipeline", StringComparison.OrdinalIgnoreCase))
        {
            return args[0].Equals("qc", StringComparison.OrdinalIgnoreCase)
                   && args.Count >= 2
                   && args[1].Equals("analyze", StringComparison.OrdinalIgnoreCase);
        }

        if (args[1].Equals("run", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return args.Count >= 3
               && args[1].Equals("prep", StringComparison.OrdinalIgnoreCase)
               && args[2].Equals("stage", StringComparison.OrdinalIgnoreCase);
    }

    private static string[] ReplacePlaceholders(string[] args, FileInfo chapter)
    {
        var replaced = new string[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            replaced[i] = args[i]
                .Replace("{audio}", chapter.FullName, StringComparison.OrdinalIgnoreCase)
                .Replace("{chapter}", Path.GetFileNameWithoutExtension(chapter.Name),
                    StringComparison.OrdinalIgnoreCase)
                .Replace("{chapterName}", Path.GetFileNameWithoutExtension(chapter.Name),
                    StringComparison.OrdinalIgnoreCase);
        }

        return replaced;
    }
}
