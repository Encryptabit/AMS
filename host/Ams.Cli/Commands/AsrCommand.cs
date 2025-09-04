using System.CommandLine;
using System.Linq;
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

        // Silence detection command
        asrCommand.AddCommand(CreateSilenceCommand());
        // Planning command (deterministic DP over silence windows)
        asrCommand.AddCommand(CreatePlanCommand());
        return asrCommand;
    }

    private static Command CreateSilenceCommand()
    {
        var cmd = new Command("silence", "Detect silence windows using ffmpeg silencedetect (deterministic)");
        var audioOption = new Option<FileInfo>("--audio", "Path to WAV/Audio file") { IsRequired = true };
        audioOption.AddAlias("-a");
        var outDirOption = new Option<DirectoryInfo>("--out", () => new DirectoryInfo("."), "Output directory for artifacts");
        outDirOption.AddAlias("-o");
        var dbFloorOption = new Option<double>("--db-floor", () => -30.0, "Silence noise floor in dBFS (e.g., -30)");
        var minDurOption = new Option<double>("--min-silence-dur", () => 0.3, "Minimum silence duration in seconds");

        cmd.AddOption(audioOption);
        cmd.AddOption(outDirOption);
        cmd.AddOption(dbFloorOption);
        cmd.AddOption(minDurOption);

        cmd.SetHandler(async (audio, outDir, dbFloor, minDur) =>
        {
            var audioPath = audio!.FullName;
            var outRoot = outDir!.FullName;
            System.IO.Directory.CreateDirectory(outRoot);

            var silenceParams = new SilenceParams(dbFloor, minDur);
            var detector = new FfmpegSilenceDetector();
            var runner = new DefaultProcessRunner();

            Console.WriteLine($"Detecting silences with ffmpeg (db-floor={dbFloor} dB, min-dur={minDur}s)...");
            SilenceTimeline timeline;
            try
            {
                timeline = await detector.DetectAsync(audioPath, silenceParams, runner);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ffmpeg silencedetect failed: {ex.Message}");
                Environment.Exit(1);
                return;
            }

            // Save timeline JSON deterministically next to output
            var baseName = System.IO.Path.GetFileNameWithoutExtension(audioPath);
            var silencePath = System.IO.Path.Combine(outRoot, baseName + ".silence.json");
            var json = System.Text.Json.JsonSerializer.Serialize(timeline, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            await System.IO.File.WriteAllTextAsync(silencePath, json);
            Console.WriteLine($"Silence timeline saved: {silencePath}");
        }, audioOption, outDirOption, dbFloorOption, minDurOption);

        return cmd;
    }

    private static Command CreatePlanCommand()
    {
        var cmd = new Command("plan", "Plan deterministic chunk spans (only at silence windows) with 60–90s constraints, target 75s");
        var audioOption = new Option<FileInfo>("--audio", "Path to WAV/Audio file") { IsRequired = true };
        audioOption.AddAlias("-a");
        var outDirOption = new Option<DirectoryInfo>("--out", () => new DirectoryInfo("."), "Output directory for artifacts");
        outDirOption.AddAlias("-o");
        var silenceFileOption = new Option<FileInfo>("--silences", description: "Silence timeline JSON path (from 'asr silence')") { IsRequired = true };
        silenceFileOption.AddAlias("-s");
        var minOpt = new Option<double>("--win-min-sec", () => 60.0, "Minimum chunk duration (sec)");
        var maxOpt = new Option<double>("--win-max-sec", () => 90.0, "Maximum chunk duration (sec)");
        var targetOpt = new Option<double>("--win-target-sec", () => 75.0, "Target chunk duration (sec)");
        var strictTailOpt = new Option<bool>("--strict-tail", () => true, "If true, fail when end cannot be reached with strict bounds");

        cmd.AddOption(audioOption);
        cmd.AddOption(outDirOption);
        cmd.AddOption(silenceFileOption);
        cmd.AddOption(minOpt);
        cmd.AddOption(maxOpt);
        cmd.AddOption(targetOpt);
        cmd.AddOption(strictTailOpt);

        cmd.SetHandler(async (audio, outDir, silencesFile, min, max, target, strictTail) =>
        {
            var audioPath = audio!.FullName;
            var outRoot = outDir!.FullName;
            System.IO.Directory.CreateDirectory(outRoot);

            // Load silence timeline
            SilenceTimeline timeline;
            try
            {
                var json = await System.IO.File.ReadAllTextAsync(silencesFile!.FullName);
                timeline = System.Text.Json.JsonSerializer.Deserialize<SilenceTimeline>(json) ?? throw new InvalidOperationException("Invalid silence JSON");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to read silences: {ex.Message}");
                Environment.Exit(1);
                return;
            }

            // Determine duration from WAV header via WavIo
            double durationSec;
            try
            {
                var buf = WavIo.ReadPcmOrFloat(audioPath);
                durationSec = buf.Length / (double)buf.SampleRate;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to read audio duration: {ex.Message}");
                Environment.Exit(1);
                return;
            }

            var planner = new SilenceWindowPlanner();
            var seg = new SegmentationParams(min, max, target, strictTail);
            var candidates = timeline.Events.Select(e => e.Mid).ToArray();
            ChunkPlan plan;
            try
            {
                plan = planner.Plan(durationSec, candidates, seg);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Planning failed: {ex.Message}");
                Environment.Exit(2);
                return;
            }

            // Build a simple manifest scaffold
            var baseName = System.IO.Path.GetFileNameWithoutExtension(audioPath);
            var manifestPath = System.IO.Path.Combine(outRoot, baseName + ".asr.index.json");
            var chunks = new List<ChunkEntry>();
            foreach (var span in plan.Spans)
            {
                var id = $"chunk-{(int)Math.Round(span.Start*1000):D6}-{(int)Math.Round(span.End*1000):D6}";
                chunks.Add(new ChunkEntry(span, id, null, null, "pending", 0, null));
            }
            var manifest = new AsrManifest(
                audioPath,
                timeline.AudioSha256,
                outRoot,
                seg,
                timeline.Params,
                System.IO.Path.Combine(outRoot, baseName + ".silence.json"),
                new ToolingMeta("ffmpeg", timeline.FfmpegVersion, $"silencedetect noise={timeline.Params.DbFloor}dB:d={timeline.Params.MinSilenceDur}"),
                chunks
            );

            var manifestJson = System.Text.Json.JsonSerializer.Serialize(manifest, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            await System.IO.File.WriteAllTextAsync(manifestPath, manifestJson);
            Console.WriteLine($"Planned {plan.Spans.Count} chunks. Manifest: {manifestPath}");
        }, audioOption, outDirOption, silenceFileOption, minOpt, maxOpt, targetOpt, strictTailOpt);

        return cmd;
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
