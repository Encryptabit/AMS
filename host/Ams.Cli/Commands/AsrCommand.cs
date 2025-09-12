using System.CommandLine;
using Ams.Core;

namespace Ams.Cli.Commands;

public static class AsrCommand
{
    public static Command Create()
    {
        var asrCommand = new Command("asr", "ASR (Automatic Speech Recognition) operations");
         
        // Delegate the 'run' subcommand to AsrRunCommand (pipeline orchestrator)
        asrCommand.AddCommand(AsrRunCommand.Create());

        // Staged pipeline commands (also available at root via Program)
        asrCommand.AddCommand(DetectSilenceCommand.Create());
        asrCommand.AddCommand(PlanWindowsCommand.Create());
        asrCommand.AddCommand(AlignChunksCommand.Create());
        asrCommand.AddCommand(RefineCommand.Create());
        asrCommand.AddCommand(CollateCommand.Create());
        return asrCommand;
    }

}
