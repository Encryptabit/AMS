using System.CommandLine;
using System.Text.Json;
using Ams.Core;
using Ams.Core.Common;

namespace Ams.Cli.Commands;

public static class ValidateCommand
{
    public static Command Create()
    {
        var validateCommand = new Command("validate", "Validation operations");
        
        var scriptCommand = new Command("script", "Validate transcript against script");
        
        var audioOption = new Option<FileInfo>("--audio", "Path to the audio file")
        {
            IsRequired = true
        };
        audioOption.AddAlias("-a");
        
        var scriptOption = new Option<FileInfo>("--script", "Path to the script text file")
        {
            IsRequired = true
        };
        scriptOption.AddAlias("-s");
        
        var asrJsonOption = new Option<FileInfo>("--asr-json", "Path to the ASR JSON file")
        {
            IsRequired = true
        };
        asrJsonOption.AddAlias("-j");
        
        var outputOption = new Option<FileInfo>("--out", "Output validation report JSON file")
        {
            IsRequired = true
        };
        outputOption.AddAlias("-o");
        
        var substitutionCostOption = new Option<double>("--sub-cost", () => 1.0, "Substitution cost for alignment");
        var insertionCostOption = new Option<double>("--ins-cost", () => 1.0, "Insertion cost for alignment");
        var deletionCostOption = new Option<double>("--del-cost", () => 1.0, "Deletion cost for alignment");
        var expandContractionsOption = new Option<bool>("--expand-contractions", () => true, "Expand contractions during normalization");
        var removeNumbersOption = new Option<bool>("--remove-numbers", () => false, "Remove numbers during normalization");
        
        scriptCommand.AddOption(audioOption);
        scriptCommand.AddOption(scriptOption);
        scriptCommand.AddOption(asrJsonOption);
        scriptCommand.AddOption(outputOption);
        scriptCommand.AddOption(substitutionCostOption);
        scriptCommand.AddOption(insertionCostOption);
        scriptCommand.AddOption(deletionCostOption);
        scriptCommand.AddOption(expandContractionsOption);
        scriptCommand.AddOption(removeNumbersOption);
        
        scriptCommand.SetHandler(async (context) =>
        {
            try
            {
                var audio = context.ParseResult.GetValueForOption(audioOption)!;
                var script = context.ParseResult.GetValueForOption(scriptOption)!;
                var asrJson = context.ParseResult.GetValueForOption(asrJsonOption)!;
                var output = context.ParseResult.GetValueForOption(outputOption)!;
                var subCost = context.ParseResult.GetValueForOption(substitutionCostOption);
                var insCost = context.ParseResult.GetValueForOption(insertionCostOption);
                var delCost = context.ParseResult.GetValueForOption(deletionCostOption);
                var expandContractions = context.ParseResult.GetValueForOption(expandContractionsOption);
                var removeNumbers = context.ParseResult.GetValueForOption(removeNumbersOption);
                
                await ValidateScriptAsync(audio, script, asrJson, output, subCost, insCost, delCost, expandContractions, removeNumbers);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "validate script command failed");
                Environment.Exit(1);
            }
        });
        
        validateCommand.AddCommand(scriptCommand);
        return validateCommand;
    }
    
    private static async Task ValidateScriptAsync(
        FileInfo audioFile,
        FileInfo scriptFile,
        FileInfo asrJsonFile,
        FileInfo outputFile,
        double substitutionCost,
        double insertionCost,
        double deletionCost,
        bool expandContractions,
        bool removeNumbers)
    {
        Log.Info(
            "Validating transcript using audio {AudioFile}, script {ScriptFile}, asr {AsrJsonFile}, output {OutputFile}",
            audioFile.FullName,
            scriptFile.FullName,
            asrJsonFile.FullName,
            outputFile.FullName);
        
        var options = new ValidationOptions
        {
            SubstitutionCost = substitutionCost,
            InsertionCost = insertionCost,
            DeletionCost = deletionCost,
            ExpandContractions = expandContractions,
            RemoveNumbers = removeNumbers
        };
        
        var validator = new ScriptValidator(options);
        
        Log.Info("Running validation alignment");
        var report = await validator.ValidateAsync(audioFile.FullName, scriptFile.FullName, asrJsonFile.FullName);
        Log.Info("Validation complete");
        
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        await File.WriteAllTextAsync(outputFile.FullName, json);
        
        Log.Info(
            "Validation summary: WER {WordErrorRate:P2}, CER {CharacterErrorRate:P2}, TotalWords {TotalWords}, Correct {CorrectWords}, Substitutions {Substitutions}, Insertions {Insertions}, Deletions {Deletions}, Findings {FindingsCount}",
            report.WordErrorRate,
            report.CharacterErrorRate,
            report.TotalWords,
            report.CorrectWords,
            report.Substitutions,
            report.Insertions,
            report.Deletions,
            report.Findings.Length);
        
        var findingsByType = report.Findings.GroupBy(f => f.Type).ToList();
        if (findingsByType.Count > 0)
        {
            foreach (var group in findingsByType)
            {
                Log.Info("Finding type {FindingType}: {FindingCount}", group.Key, group.Count());
            }
        }
        
        if (report.SegmentStats.Length > 0)
        {
            var avgSegmentWer = report.SegmentStats.Average(s => s.WordErrorRate);
            var confidences = report.SegmentStats.Where(s => s.Confidence.HasValue).Select(s => s.Confidence!.Value).ToList();
            Log.Info("Segment analysis: Count {SegmentCount}, AvgWER {AverageWer:P2}", report.SegmentStats.Length, avgSegmentWer);
            if (confidences.Count > 0)
            {
                Log.Info("Segment confidence average {AverageConfidence:F3}", confidences.Average());
            }
        }
        
        Log.Info("Detailed validation report saved to {OutputFile}", outputFile.FullName);
    }
}