using System.CommandLine;
using System.Text.Json;
using Ams.Core;

namespace Ams.Cli.Commands;

public static class AsrCommand
{
    public static Command Create()
    {
        var asrCommand = new Command("asr", "ASR (Automatic Speech Recognition) operations");
        
        var runCommand = new Command("run", "Run ASR on an audio file");
        
        var audioOption = new Option<FileInfo>("--audio", "Path to the audio file (WAV format)")
        {
            IsRequired = true
        };
        audioOption.AddAlias("-a");
        
        var outputOption = new Option<FileInfo>("--out", "Output ASR JSON file")
        {
            IsRequired = true
        };
        outputOption.AddAlias("-o");
        
        var serviceUrlOption = new Option<string>("--service", () => "http://localhost:8000", "ASR service URL");
        serviceUrlOption.AddAlias("-s");
        
        var modelOption = new Option<string>("--model", "ASR model to use (optional)");
        modelOption.AddAlias("-m");
        
        var languageOption = new Option<string>("--language", () => "en", "Language code");
        languageOption.AddAlias("-l");
        
        runCommand.AddOption(audioOption);
        runCommand.AddOption(outputOption);
        runCommand.AddOption(serviceUrlOption);
        runCommand.AddOption(modelOption);
        runCommand.AddOption(languageOption);
        
        runCommand.SetHandler(async (audio, output, serviceUrl, model, language) =>
        {
            try
            {
                await RunAsrAsync(audio, output, serviceUrl, model, language);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }, audioOption, outputOption, serviceUrlOption, modelOption, languageOption);
        
        asrCommand.AddCommand(runCommand);
        return asrCommand;
    }
    
    private static async Task RunAsrAsync(FileInfo audioFile, FileInfo outputFile, string serviceUrl, string? model, string language)
    {
        if (!audioFile.Exists)
        {
            throw new FileNotFoundException($"Audio file not found: {audioFile.FullName}");
        }
        
        Console.WriteLine($"Running ASR on: {audioFile.FullName}");
        Console.WriteLine($"Service URL: {serviceUrl}");
        Console.WriteLine($"Language: {language}");
        if (model != null) Console.WriteLine($"Model: {model}");
        
        using var client = new AsrClient(serviceUrl);
        
        // Check service health first
        Console.Write("Checking ASR service health... ");
        var isHealthy = await client.IsHealthyAsync();
        if (!isHealthy)
        {
            throw new InvalidOperationException($"ASR service at {serviceUrl} is not healthy or unreachable");
        }
        Console.WriteLine("OK");
        
        // Run transcription
        Console.Write("Transcribing audio... ");
        var response = await client.TranscribeAsync(audioFile.FullName, model, language);
        Console.WriteLine("Done");
        
        // Save result
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        await File.WriteAllTextAsync(outputFile.FullName, json);
        
        Console.WriteLine($"Results saved to: {outputFile.FullName}");
        Console.WriteLine($"Model version: {response.ModelVersion}");
        Console.WriteLine($"Total words: {response.Tokens.Length}");
    }
}