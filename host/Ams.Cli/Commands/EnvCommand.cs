using System.CommandLine;

namespace Ams.Cli.Commands;

public static class EnvCommand
{
    public static Command Create()
    {
        var cmd = new Command("env", "Environment validation and service checks");

        // Add the aeneas-validate subcommand
        cmd.AddCommand(AeneasValidateCommand.Create());

        return cmd;
    }
}