using System.CommandLine;
using System.Text;
using Ams.Core.Common;
using Ams.Cli.Commands;

namespace Ams.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        using var loggerFactory = Log.ConfigureDefaults(logFileName: "ams-log.txt");
        Log.Info("Structured logging initialized. Console + file at {LogFile}", Log.LogFilePath ?? "(unknown)");

        var rootCommand = new RootCommand("AMS - Audio Management System CLI");

        rootCommand.AddCommand(AsrCommand.Create());
        rootCommand.AddCommand(ValidateCommand.Create());
        rootCommand.AddCommand(TextCommand.Create());
        rootCommand.AddCommand(BuildIndexCommand.Create());
        rootCommand.AddCommand(BookCommand.Create());
        rootCommand.AddCommand(AlignCommand.Create());
        rootCommand.AddCommand(AudioCommand.Create());
        rootCommand.AddCommand(RefineSentencesCommand.Create());
        rootCommand.AddCommand(PipelineCommand.Create());
        
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
        Console.WriteLine("AMS Interactive CLI - Type 'help' for commands, 'exit' to quit");

        while (true)
        {
            Console.Write("[AMS] > ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input)) continue;
            
            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Exiting...");
                break;
            }

            if (input.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                await rootCommand.InvokeAsync(new[] { "--help" });
            }

            if (input.Equals("clear", StringComparison.OrdinalIgnoreCase))
            {
                Console.Clear();
            }

            try
            {
                //Parse input as command line arguments
                var replArgs = ParseInput(input);
                await rootCommand.InvokeAsync(replArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            
            Console.WriteLine();
        }

    }
    
    static string[] ParseInput(string input)
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
                    args.Add(current.ToString());
                    current.Clear();
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
}
