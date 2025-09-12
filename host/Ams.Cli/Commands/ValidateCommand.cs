using System.CommandLine;
using System.Text.Json;
using Ams.Core;

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
                Console.Error.WriteLine($"Error: {ex.Message}");
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
        Console.WriteLine($"Validating transcript against script...");
        Console.WriteLine($"Audio file: {audioFile.FullName}");
        Console.WriteLine($"Script file: {scriptFile.FullName}");
        Console.WriteLine($"ASR JSON file: {asrJsonFile.FullName}");
        Console.WriteLine($"Output file: {outputFile.FullName}");
        
        var options = new ValidationOptions
        {
            SubstitutionCost = substitutionCost,
            InsertionCost = insertionCost,
            DeletionCost = deletionCost,
            ExpandContractions = expandContractions,
            RemoveNumbers = removeNumbers
        };
        
        var validator = new ScriptValidator(options);
        
        Console.Write("Running validation... ");
        var report = await validator.ValidateAsync(audioFile.FullName, scriptFile.FullName, asrJsonFile.FullName);
        Console.WriteLine("Done");
        
        // Save report
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        await File.WriteAllTextAsync(outputFile.FullName, json);
        
        // Print summary
        Console.WriteLine("\n=== Validation Results ===");
        Console.WriteLine($"Word Error Rate: {report.WordErrorRate:P2}");
        Console.WriteLine($"Character Error Rate: {report.CharacterErrorRate:P2}");
        Console.WriteLine($"Total words: {report.TotalWords}");
        Console.WriteLine($"Correct words: {report.CorrectWords}");
        Console.WriteLine($"Substitutions: {report.Substitutions}");
        Console.WriteLine($"Insertions: {report.Insertions}");
        Console.WriteLine($"Deletions: {report.Deletions}");
        Console.WriteLine($"Total findings: {report.Findings.Length}");
        
        // Print findings summary by type
        var findingsByType = report.Findings.GroupBy(f => f.Type).ToList();
        if (findingsByType.Count > 0)
        {
            Console.WriteLine("\nFindings by type:");
            foreach (var group in findingsByType)
            {
                Console.WriteLine($"  {group.Key}: {group.Count()}");
            }
        }
        
        // Print segment stats summary
        if (report.SegmentStats.Length > 0)
        {
            var avgSegmentWer = report.SegmentStats.Average(s => s.WordErrorRate);
            var avgConfidence = report.SegmentStats.Where(s => s.Confidence.HasValue).Average(s => s.Confidence!.Value);
            
            Console.WriteLine($"\nSegment analysis:");
            Console.WriteLine($"  Total segments: {report.SegmentStats.Length}");
            Console.WriteLine($"  Average segment WER: {avgSegmentWer:P2}");
            if (report.SegmentStats.Any(s => s.Confidence.HasValue))
            {
                Console.WriteLine($"  Average confidence: {avgConfidence:F3}");
            }
        }
        
        Console.WriteLine($"\nDetailed report saved to: {outputFile.FullName}");
    }
}