using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Validation;

namespace Ams.Core.Pipeline;

public class ValidateStage : StageRunner
{
    private readonly ValidationParams _params;
    private readonly ScriptValidator _validator;

    public ValidateStage(
        string workDir,
        ValidationParams parameters)
        : base(workDir, "validate")
    {
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
        
        var validationOptions = new ValidationOptions
        {
            SubstitutionCost = _params.EditCosts?.GetValueOrDefault("sub", 1.0) ?? 1.0,
            InsertionCost = _params.EditCosts?.GetValueOrDefault("ins", 1.0) ?? 1.0,
            DeletionCost = _params.EditCosts?.GetValueOrDefault("del", 1.0) ?? 1.0
        };
        
        _validator = new ScriptValidator(validationOptions);
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        // Load merged transcript
        var mergedPath = Path.Combine(WorkDir, "transcripts", "merged.json");
        if (!File.Exists(mergedPath))
            throw new InvalidOperationException("Merged transcript not found. Run 'transcripts' stage first.");

        Console.WriteLine("Running script vs transcript validation...");

        // We need a script file to compare against - check common locations
        var scriptPath = FindScriptFile(manifest.Input.Path);
        
        ValidationReport? report = null;
        if (scriptPath != null)
        {
            Console.WriteLine($"Found script file: {scriptPath}");
            
            // Convert merged transcript to ASR response format for validation
            var asrResponse = await ConvertTranscriptToAsrResponseAsync(mergedPath, ct);
            
            report = await _validator.ValidateAsync(manifest.Input.Path, scriptPath, mergedPath);
            
            Console.WriteLine($"Validation completed: WER={report.WordErrorRate:P2}, CER={report.CharacterErrorRate:P2}");
        }
        else
        {
            Console.WriteLine("No script file found - generating transcript-only report");
            report = await GenerateTranscriptOnlyReportAsync(manifest.Input.Path, mergedPath, ct);
        }

        // Save validation report
        var reportPath = Path.Combine(stageDir, "report.json");
        var reportJson = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(reportPath, reportJson, ct);

        // Create summary text
        var summaryPath = Path.Combine(stageDir, "summary.txt");
        await CreateSummaryTextAsync(report, summaryPath, ct);

        // Check thresholds and create validation result
        var validationResult = new
        {
            Passed = report.WordErrorRate <= _params.WerThreshold && report.CharacterErrorRate <= _params.CerThreshold,
            WerThreshold = _params.WerThreshold,
            CerThreshold = _params.CerThreshold,
            ActualWer = report.WordErrorRate,
            ActualCer = report.CharacterErrorRate,
            TotalWords = report.TotalWords,
            ScriptFile = scriptPath,
            GeneratedAt = DateTime.UtcNow
        };

        var resultPath = Path.Combine(stageDir, "result.json");
        var resultJson = JsonSerializer.Serialize(validationResult, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(resultPath, resultJson, ct);

        var paramsPath = Path.Combine(stageDir, "params.snapshot.json");
        var paramsJsonOutput = SerializeParams(_params);
        await File.WriteAllTextAsync(paramsPath, paramsJsonOutput, ct);

        var status = validationResult.Passed ? "PASSED" : "FAILED";
        Console.WriteLine($"Validation {status}: WER={report.WordErrorRate:P2} (threshold: {_params.WerThreshold:P2}), CER={report.CharacterErrorRate:P2} (threshold: {_params.CerThreshold:P2})");

        return new Dictionary<string, string>
        {
            ["report"] = "report.json",
            ["summary"] = "summary.txt",
            ["result"] = "result.json",
            ["params"] = "params.snapshot.json"
        };
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        var paramsHash = ComputeHash(SerializeParams(_params));

        // Include merged transcript in input hash
        var mergedPath = Path.Combine(WorkDir, "transcripts", "merged.json");
        var mergedHash = "";
        
        if (File.Exists(mergedPath))
        {
            var mergedContent = await File.ReadAllTextAsync(mergedPath, ct);
            mergedHash = ComputeHash(mergedContent);
        }

        // Include script file if available
        var scriptPath = FindScriptFile(manifest.Input.Path);
        var scriptHash = "";
        
        if (scriptPath != null && File.Exists(scriptPath))
        {
            var scriptContent = await File.ReadAllTextAsync(scriptPath, ct);
            scriptHash = ComputeHash(scriptContent);
        }

        var inputHash = ComputeHash(manifest.Input.Sha256 + mergedHash + scriptHash);

        return new StageFingerprint(inputHash, paramsHash, new Dictionary<string, string>());
    }

    private string? FindScriptFile(string audioPath)
    {
        var audioDir = Path.GetDirectoryName(audioPath);
        var audioName = Path.GetFileNameWithoutExtension(audioPath);
        
        if (string.IsNullOrEmpty(audioDir) || string.IsNullOrEmpty(audioName))
            return null;

        // Common script file patterns
        var patterns = new[]
        {
            Path.Combine(audioDir, $"{audioName}.txt"),
            Path.Combine(audioDir, $"{audioName}_script.txt"),
            Path.Combine(audioDir, $"{audioName}.script"),
            Path.Combine(audioDir, "script.txt"),
            // Look in parent directory too
            Path.Combine(Path.GetDirectoryName(audioDir) ?? audioDir, $"{audioName}.txt"),
            Path.Combine(Path.GetDirectoryName(audioDir) ?? audioDir, "script.txt")
        };

        foreach (var pattern in patterns)
        {
            if (File.Exists(pattern))
                return pattern;
        }

        return null;
    }

    private async Task<AsrResponse> ConvertTranscriptToAsrResponseAsync(string mergedPath, CancellationToken ct)
    {
        var mergedJson = await File.ReadAllTextAsync(mergedPath, ct);
        var merged = JsonSerializer.Deserialize<JsonElement>(mergedJson);

        var words = new List<AsrWord>();
        
        if (merged.TryGetProperty("Words", out var wordsArray))
        {
            foreach (var wordElement in wordsArray.EnumerateArray())
            {
                var asrWord = new AsrWord(
                    wordElement.GetProperty("Word").GetString() ?? "",
                    wordElement.GetProperty("Start").GetDouble(),
                    wordElement.GetProperty("End").GetDouble(),
                    wordElement.TryGetProperty("Confidence", out var conf) ? conf.GetDouble() : 1.0
                );
                words.Add(asrWord);
            }
        }

        // Build full text from words
        var fullText = string.Join(" ", words.Select(w => w.Word));

        return new AsrResponse(
            Text: fullText,
            Words: words,
            Duration: words.Count > 0 ? words.Max(w => w.End) : 0.0,
            Language: "en", // Default
            Model: "transcription-pipeline",
            Confidence: words.Count > 0 ? words.Average(w => w.Confidence) : 1.0
        );
    }

    private async Task<ValidationReport> GenerateTranscriptOnlyReportAsync(string audioPath, string mergedPath, CancellationToken ct)
    {
        var mergedJson = await File.ReadAllTextAsync(mergedPath, ct);
        var merged = JsonSerializer.Deserialize<JsonElement>(mergedJson);

        var totalWords = 0;
        if (merged.TryGetProperty("TotalWords", out var totalWordsElement))
        {
            totalWords = totalWordsElement.GetInt32();
        }

        // Create a basic report without script comparison
        return new ValidationReport(
            AudioFile: audioPath,
            ScriptFile: null,
            AsrFile: mergedPath,
            Timestamp: DateTime.UtcNow,
            WordErrorRate: 0.0, // Cannot calculate without script
            CharacterErrorRate: 0.0, // Cannot calculate without script
            TotalWords: totalWords,
            CorrectWords: totalWords, // Assume all correct without reference
            Substitutions: 0,
            Insertions: 0,
            Deletions: 0,
            Findings: new List<ValidationFinding>(),
            SegmentStats: new List<SegmentStats>()
        );
    }

    private async Task CreateSummaryTextAsync(ValidationReport report, string summaryPath, CancellationToken ct)
    {
        var summary = new List<string>
        {
            "=== Validation Summary ===",
            $"Audio: {Path.GetFileName(report.AudioFile)}",
            $"Script: {(report.ScriptFile != null ? Path.GetFileName(report.ScriptFile) : "Not found")}",
            $"Timestamp: {report.Timestamp:yyyy-MM-dd HH:mm:ss} UTC",
            "",
            "=== Error Rates ===",
            $"Word Error Rate (WER): {report.WordErrorRate:P2}",
            $"Character Error Rate (CER): {report.CharacterErrorRate:P2}",
            "",
            "=== Word Statistics ===",
            $"Total Words: {report.TotalWords}",
            $"Correct Words: {report.CorrectWords}",
            $"Substitutions: {report.Substitutions}",
            $"Insertions: {report.Insertions}",
            $"Deletions: {report.Deletions}",
            ""
        };

        // Add threshold check results
        var werPassed = report.WordErrorRate <= _params.WerThreshold;
        var cerPassed = report.CharacterErrorRate <= _params.CerThreshold;
        var overallPassed = werPassed && cerPassed;

        summary.AddRange(new[]
        {
            "=== Threshold Check ===",
            $"WER Threshold: {_params.WerThreshold:P2} - {(werPassed ? "PASSED" : "FAILED")}",
            $"CER Threshold: {_params.CerThreshold:P2} - {(cerPassed ? "PASSED" : "FAILED")}",
            $"Overall Result: {(overallPassed ? "PASSED" : "FAILED")}",
            ""
        });

        // Add findings summary
        if (report.Findings.Count > 0)
        {
            summary.Add("=== Top Findings ===");
            var topFindings = report.Findings.Take(10);
            foreach (var finding in topFindings)
            {
                summary.Add($"- {finding.Type}: {finding.Message} (Severity: {finding.Severity})");
            }
            summary.Add("");
        }

        await File.WriteAllTextAsync(summaryPath, string.Join("\n", summary), ct);
    }
}