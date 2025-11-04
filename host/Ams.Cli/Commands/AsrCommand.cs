using System.CommandLine;
using System.Text.Json;
using Ams.Core;
using Ams.Core.Common;
using Ams.Cli.Utilities;

namespace Ams.Cli.Commands;

public static class AsrCommand
{
    internal const string DefaultServiceUrl = "http://localhost:8000";

    public static Command Create()
    {
        var asrCommand = new Command("asr", "ASR (Automatic Speech Recognition) operations");
        
        var runCommand = new Command("run", "Run ASR on an audio file");
        
        var audioOption = new Option<FileInfo?>("--audio", "Path to the audio file (WAV format)");
        audioOption.AddAlias("-a");

        var outputOption = new Option<FileInfo?>("--out", "Output ASR JSON file");
        outputOption.AddAlias("-o");
        
        var serviceUrlOption = new Option<string>("--service", () => DefaultServiceUrl, "ASR service URL");
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
                var audioFile = CommandInputResolver.RequireAudio(audio);
                var outputFile = CommandInputResolver.ResolveOutput(output, "asr.json");
                await RunAsrAsync(audioFile, outputFile, serviceUrl, model, language);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "asr run command failed");
                Environment.Exit(1);
            }
        }, audioOption, outputOption, serviceUrlOption, modelOption, languageOption);
        
        asrCommand.AddCommand(runCommand);
        return asrCommand;
    }
    
    internal static async Task RunAsrAsync(FileInfo audioFile, FileInfo outputFile, string serviceUrl, string? model, string language)
    {
        if (!audioFile.Exists)
        {
            throw new FileNotFoundException($"Audio file not found: {audioFile.FullName}");
        }

        Log.Debug("Running ASR for {AudioFile} -> {OutputFile} via {ServiceUrl}", audioFile.FullName, outputFile.FullName, serviceUrl);
        Log.Debug("ASR parameters: Language={Language}, Model={Model}", language, model ?? "(default)");

        await AsrProcessSupervisor.EnsureServiceReadyAsync(serviceUrl, CancellationToken.None);

        using var client = new AsrClient(serviceUrl);

        Log.Debug("Checking ASR service health at {ServiceUrl}", serviceUrl);
        var isHealthy = await client.IsHealthyAsync();
        if (!isHealthy)
        {
            throw new InvalidOperationException($"ASR service at {serviceUrl} is not healthy or unreachable");
        }
        Log.Debug("ASR service responded healthy");

        Log.Debug("Submitting audio for transcription");
        var response = await client.TranscribeAsync(audioFile.FullName, model, language);
        Log.Debug("Transcription complete");

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await File.WriteAllTextAsync(outputFile.FullName, json);

        Log.Debug("ASR results written to {OutputFile}", outputFile.FullName);
        Log.Debug("ASR summary: ModelVersion={ModelVersion}, Tokens={TokenCount}", response.ModelVersion, response.Tokens.Length);
    }
}
