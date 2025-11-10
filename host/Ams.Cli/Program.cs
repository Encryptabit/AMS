using System.CommandLine;
using System.Text;
using Ams.Core.Asr;
using Ams.Core.Common;
using Ams.Cli.Repl;
using Ams.Cli.Commands;
using Ams.Cli.Services;

namespace Ams.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        using var loggerFactory = Log.ConfigureDefaults(logFileName: "ams-log.txt");
        Log.Debug("Structured logging initialized. Console + file at {LogFile}", Log.LogFilePath ?? "(unknown)");

        AsrProcessSupervisor.RegisterForShutdown();
        var configuredEngine = AsrEngineConfig.Resolve();
        Log.Debug("ASR engine configured as {Engine}", configuredEngine);
        if (configuredEngine == AsrEngine.Nemo)
        {
            var defaultAsrUrl = ResolveDefaultAsrUrl();
            AsrProcessSupervisor.TriggerBackgroundWarmup(defaultAsrUrl);
        }
        else
        {
            Log.Debug("Whisper.NET in-process ASR selected; skipping external service warmup.");
        }

        MfaProcessSupervisor.RegisterForShutdown();
        MfaProcessSupervisor.TriggerBackgroundWarmup();

        var rootCommand = new RootCommand("AMS - Audio Management System CLI");

        rootCommand.AddCommand(AsrCommand.Create());
        rootCommand.AddCommand(ValidateCommand.Create());
        rootCommand.AddCommand(TextCommand.Create());
        rootCommand.AddCommand(BuildIndexCommand.Create());
        rootCommand.AddCommand(BookCommand.Create());
        rootCommand.AddCommand(AlignCommand.Create());
        rootCommand.AddCommand(RefineSentencesCommand.Create());
        rootCommand.AddCommand(PipelineCommand.Create());
        rootCommand.AddCommand(DspCommand.Create());
        
        var replCommand = new Command("repl", "Start interactive REPL");
        replCommand.SetHandler(async () => await StartRepl(rootCommand));
        rootCommand.AddCommand(replCommand);

        // if no args , repl by default
        if (args.Length == 0)
        {
            await StartRepl(rootCommand);
            return 0;
        }
        
        return await rootCommand.InvokeAsync(args);
    }

    private static async Task StartRepl(RootCommand rootCommand)
    {
        var state = new ReplState();
        Console.WriteLine("AMS Interactive CLI - Type 'help' for CLI verbs, 'exit' to quit");
        Console.WriteLine("Built-ins: set-dir, list-wav, use, mode, run, state, clear");

        while (true)
        {
            Console.Write(Prompt(state));
            var input = Console.ReadLine();
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

            if (await TryHandleBuiltInAsync(input, state, rootCommand))
            {
                Console.WriteLine();
                continue;
            }

            try
            {
                var replArgs = ParseInput(input);
                await ExecuteWithScopeAsync(replArgs, state, rootCommand);
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
        var asrLabel = AsrProcessSupervisor.StatusLabel;
        return $"[AMS|{dirLabel}|{scopeLabel}|{asrLabel}]> ";
    }

    private static bool IsExit(string input)
    {
        return input.Equals("exit", StringComparison.OrdinalIgnoreCase)
               || input.Equals("quit", StringComparison.OrdinalIgnoreCase)
               || input.Equals("q", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveDefaultAsrUrl()
    {
        return Environment.GetEnvironmentVariable("AMS_ASR_SERVICE_URL") ?? AsrCommand.DefaultServiceUrl;
    }

    private static async Task<bool> TryHandleBuiltInAsync(string input, ReplState state, RootCommand rootCommand)
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
                await rootCommand.InvokeAsync(new[] { "--help" });
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
                await ExecuteWithScopeAsync(runArgs, state, rootCommand);
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

    private static async Task ExecuteWithScopeAsync(string[] args, ReplState state, RootCommand rootCommand)
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
                if (ShouldHandleAllChaptersInBulk(args))
                {
                    ReplContext.Current = state;
                    try
                    {
                        await rootCommand.InvokeAsync(args);
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
                            var exitCode = await rootCommand.InvokeAsync(concreteArgs);
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
                    await rootCommand.InvokeAsync(concreteArgs);
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
                    await rootCommand.InvokeAsync(args);
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

    private static bool ShouldHandleAllChaptersInBulk(IReadOnlyList<string> args)
    {
        if (args.Count < 2)
        {
            return false;
        }

        if (!args[0].Equals("pipeline", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return args[1].Equals("run", StringComparison.OrdinalIgnoreCase);
    }

    private static string[] ReplacePlaceholders(string[] args, FileInfo chapter)
    {
        var replaced = new string[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            replaced[i] = args[i]
                .Replace("{audio}", chapter.FullName, StringComparison.OrdinalIgnoreCase)
                .Replace("{chapter}", Path.GetFileNameWithoutExtension(chapter.Name), StringComparison.OrdinalIgnoreCase)
                .Replace("{chapterName}", Path.GetFileNameWithoutExtension(chapter.Name), StringComparison.OrdinalIgnoreCase);
        }

        return replaced;
    }

}
