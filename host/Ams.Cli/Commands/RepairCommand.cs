using System.CommandLine;
using Ams.Core;
using Ams.Core.Pipeline;

namespace Ams.Cli.Commands;

public static class RepairCommand
{
    public static Command Create()
    {
        var cmd = new Command("repair", "Execute auto-repair plan for failing windows");

        var workOption = new Option<DirectoryInfo>("--work", "Work directory") { IsRequired = true };
        var planOption = new Option<FileInfo>("--plan", "Path to repair plan JSON file") { IsRequired = true };
        var forceOption = new Option<bool>("--force", "Force re-run even if up-to-date");

        cmd.AddOption(workOption);
        cmd.AddOption(planOption);
        cmd.AddOption(forceOption);

        cmd.SetHandler(async (work, plan, force) =>
        {
            // Refactorv2: Step 11 - Implement repair command handler
            var workDir = work!.FullName;
            
            if (!plan!.Exists)
            {
                Console.Error.WriteLine($"Repair plan file not found: {plan.FullName}");
                Environment.Exit(1);
            }

            var json = await File.ReadAllTextAsync(plan.FullName);
            var repairPlan = System.Text.Json.JsonSerializer.Deserialize<RepairPlan>(json);
            
            if (repairPlan == null)
            {
                Console.Error.WriteLine("Invalid repair plan format");
                Environment.Exit(1);
            }

            Console.WriteLine($"Executing repair plan for {repairPlan.WindowIds.Count} windows: {repairPlan.Reason}");

            // Re-run pipeline stages for affected windows: windows → window-align → refine → collate → script-compare
            var runner = new AsrPipelineRunner();
            
            // Refactorv2: Step 11 - Fix repair command to include required dependencies
            var processRunner = new DefaultProcessRunner();
            
            // Register all stages needed for repair
            runner.RegisterStage("windows", wd => new WindowsStage(wd, new WindowsParams()));
            runner.RegisterStage("window-align", wd => new WindowAlignStage(wd, new WindowAlignParams()));
            runner.RegisterStage("refine", wd => new RefineStage(wd, new RefinementParams()));
            runner.RegisterStage("collate", wd => new CollateStage(wd, processRunner, new CollationParams()));
            runner.RegisterStage("script-compare", wd => new ScriptCompareStage(wd, new ScriptCompareParams(), ""));

            // Refactorv2: Step 11 - Use dummy input for repair stages
            var ok = await runner.RunAsync(workDir + "/dummy", workDir, fromStage: "windows", toStage: "script-compare", force: true);
            
            if (ok)
            {
                // Re-run validation to check if repair succeeded
                runner.RegisterStage("validate", wd => new ValidateStage(wd, new ValidateParams(new ValidateGates())));
                ok = await runner.RunAsync(workDir + "/dummy", workDir, fromStage: "validate", toStage: "validate", force: true);
            }

            if (!ok) Environment.Exit(1);
        }, workOption, planOption, forceOption);

        return cmd;
    }
}