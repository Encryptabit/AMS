using System.CommandLine;

namespace Ams.Cli.Commands;

public static class TextCommand
{
    public static Command Create()
    {
        var textCommand = new Command("text", "Text processing operations");
        
        var normalizeCommand = new Command("normalize", "Normalize text for comparison");
        
        var textArgument = new Argument<string>("text", "Text to normalize");
        
        var expandContractionsOption = new Option<bool>("--expand-contractions", () => true, "Expand contractions (can't -> can not)");
        var removeNumbersOption = new Option<bool>("--remove-numbers", () => false, "Remove numbers from text");
        
        normalizeCommand.AddArgument(textArgument);
        normalizeCommand.AddOption(expandContractionsOption);
        normalizeCommand.AddOption(removeNumbersOption);
        
        normalizeCommand.SetHandler((text, expandContractions, removeNumbers) =>
        {
            var result = TextNormalizer.Normalize(text, expandContractions, removeNumbers);
            Log.Debug("Normalized text: {NormalizedText}", result);
        }, textArgument, expandContractionsOption, removeNumbersOption);
        
        textCommand.AddCommand(normalizeCommand);
        return textCommand;
    }
}