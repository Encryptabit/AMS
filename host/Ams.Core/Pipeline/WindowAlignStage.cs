using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ams.Core.Pipeline;

public class WindowAlignStage : StageRunner
{
    private readonly WindowAlignParams _params;

    public WindowAlignStage(string workDir, WindowAlignParams parameters)
        : base(workDir, "window-align")
    {
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        Console.WriteLine($"Aligning windows via Aeneas (service={_params.ServiceUrl}, band={_params.BandWidthMs}ms)...");

        // Refactorv2: Step 7 - Scaffold window alignment with placeholder result
        // TODO: Call existing Aeneas client if available, otherwise create stub alignment writer
        var windowsPath = Path.Combine(WorkDir, "windows", "windows.v2.json");
        if (!File.Exists(windowsPath))
        {
            throw new InvalidOperationException("Windows not found. Run 'windows' stage first.");
        }

        var windowsContent = await File.ReadAllTextAsync(windowsPath, ct);
        var windowsResult = JsonSerializer.Deserialize<WindowsResult>(windowsContent);
        
        if (windowsResult == null)
        {
            throw new InvalidOperationException("Failed to parse windows result");
        }

        var windowToFileMap = new Dictionary<string, string>();

        // Create placeholder alignment for each window
        foreach (var window in windowsResult.Windows)
        {
            var alignmentFilename = $"{window.Id}.aeneas.json";
            var alignmentPath = Path.Combine(stageDir, alignmentFilename);
            
            // TODO: Replace with actual Aeneas service call
            var windowAlignment = new WindowAlignment(
                WindowId: window.Id,
                OffsetSec: 0.0,
                Language: _params.Language,
                TextDigest: "placeholder_text_digest",
                Fragments: new List<AlignmentFragment>
                {
                    new AlignmentFragment(0.0, 1.0),
                    new AlignmentFragment(1.0, 2.0)
                },
                ToolVersions: new Dictionary<string, string> { ["aeneas"] = "1.7.3" },
                GeneratedAt: DateTime.UtcNow
            );

            var alignmentJson = JsonSerializer.Serialize(windowAlignment, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(alignmentPath, alignmentJson, ct);
            
            windowToFileMap[window.Id] = alignmentFilename;
        }

        var indexResult = new WindowAlignIndex(
            WindowToFileMap: windowToFileMap,
            Params: _params,
            ToolVersions: new Dictionary<string, string> { ["aeneas"] = "1.7.3", ["python"] = "3.9" }
        );

        var indexPath = Path.Combine(stageDir, "index.v2.json");
        var indexJson = JsonSerializer.Serialize(indexResult, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(indexPath, indexJson, ct);

        var paramsPath = Path.Combine(stageDir, "params.snapshot.json");
        var paramsJson = SerializeParams(_params);
        await File.WriteAllTextAsync(paramsPath, paramsJson, ct);

        Console.WriteLine($"Aligned {windowsResult.Windows.Count} windows");

        return new Dictionary<string, string>
        {
            ["index"] = "index.v2.json",
            ["params"] = "params.snapshot.json"
        };
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        // Refactorv2: Step 7 - Compute fingerprint based on windows and Aeneas params
        var paramsHash = ComputeHash(SerializeParams(_params));

        // Hash windows result
        var windowsPath = Path.Combine(WorkDir, "windows", "windows.v2.json");
        var windowsHash = "placeholder_windows_hash";
        if (File.Exists(windowsPath))
        {
            var windowsContent = await File.ReadAllTextAsync(windowsPath);
            windowsHash = ComputeHash(windowsContent);
        }

        var toolVersions = new Dictionary<string, string>
        {
            ["aeneas"] = "1.7.3",
            ["python"] = "3.9"
        };

        return new StageFingerprint(
            windowsHash,
            paramsHash,
            toolVersions
        );
    }
}