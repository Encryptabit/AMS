using System.CommandLine;
using System.Text.Json;
using Ams.Core;

namespace Ams.Cli.Commands;

public static class ValidateManifestCommand
{
    public static Command Create()
    {
        var cmd = new Command("validate-manifest", "Validate ASR pipeline manifest and artifacts")
        {
            Description = "Validate ASR pipeline manifest and artifacts in a work directory"
        };

        var workOption = new Option<DirectoryInfo>("--work", "Work directory containing manifest.json") { IsRequired = true };
        cmd.AddOption(workOption);

        cmd.SetHandler(async (work) =>
        {
            var workDir = work!.FullName;
            var manifestPath = Path.Combine(workDir, "manifest.json");
            if (!File.Exists(manifestPath))
            {
                Console.Error.WriteLine($"Manifest not found: {manifestPath}");
                Environment.Exit(1);
                return;
            }

            var json = await File.ReadAllTextAsync(manifestPath);
            var manifest = JsonSerializer.Deserialize<ManifestV2>(json);
            if (manifest == null)
            {
                Console.Error.WriteLine("Invalid manifest format");
                Environment.Exit(1);
                return;
            }

            Console.WriteLine($"Validating ASR pipeline for input: {manifest.Input.Path}");
            var issues = new List<string>();
            var warnings = new List<string>();

            if (!File.Exists(manifest.Input.Path))
                issues.Add($"Input file missing: {manifest.Input.Path}");

            foreach (var (stageName, stage) in manifest.Stages)
            {
                if (stage.Status.Status != "completed") continue;
                var stageDir = Path.Combine(workDir, stageName);
                if (!Directory.Exists(stageDir))
                {
                    issues.Add($"Missing stage directory: {stageDir}");
                    continue;
                }
                foreach (var (name, relPath) in stage.Artifacts)
                {
                    var full = Path.Combine(stageDir, relPath);
                    if (!File.Exists(full))
                        issues.Add($"Missing artifact {name}: {full}");
                }
                if (!File.Exists(Path.Combine(stageDir, "meta.json")))
                    warnings.Add($"Missing meta.json for {stageName}");
                if (!File.Exists(Path.Combine(stageDir, "status.json")))
                    warnings.Add($"Missing status.json for {stageName}");
            }

            if (warnings.Count > 0)
            {
                Console.WriteLine($"⚠️  {warnings.Count} warning(s):");
                foreach (var w in warnings) Console.WriteLine($"  - {w}");
            }

            if (issues.Count > 0)
            {
                Console.WriteLine($"❌ {issues.Count} issue(s) found:");
                foreach (var i in issues) Console.WriteLine($"  - {i}");
                Environment.Exit(1);
            }
            else
            {
                Console.WriteLine("✅ Manifest validation passed");
            }
        }, workOption);

        return cmd;
    }
}
