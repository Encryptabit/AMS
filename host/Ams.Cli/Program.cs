using System.CommandLine;
using Ams.Cli.Commands;

namespace Ams.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
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
