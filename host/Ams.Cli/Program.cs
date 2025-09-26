using System.CommandLine;
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

        return await rootCommand.InvokeAsync(args);
    }
}
