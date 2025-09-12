using System.CommandLine;
using System.Text.Json;
using Ams.Core.Align.Tx;
using Ams.Core;

namespace Ams.Cli.Commands;

public static class AudioCommand
{
    public static Command Create()
    {
        var audio = new Command("audio", "Audio rendering utilities");
        audio.AddCommand(CreateRoomtone());
        return audio;
    }

    private static Command CreateRoomtone()
    {
        var cmd = new Command("roomtone", "Render WAV with roomtone-filled gaps (sentence-level) and 5 ms crossfades");

        var txOption = new Option<FileInfo>("--tx-json", description: "Path to TranscriptIndex (*.tx.json)") { IsRequired = true };
        txOption.AddAlias("-t");
        var outOption = new Option<FileInfo>("--out-wav", description: "Output WAV path") { IsRequired = true };
        outOption.AddAlias("-o");

        var srOption = new Option<int>("--sample-rate", () => 44100, "Output sample rate (Hz)");
        var bitOption = new Option<int>("--bit-depth", () => 16, "Output bit depth (currently 16 only)");
        var fadeMsOption = new Option<double>("--fade-ms", () => 5.0, "Crossfade length at boundaries (ms)");
        var toneDbOption = new Option<double>("--tone-gain-db", () => -60.0, "Roomtone RMS level (dBFS)");

        cmd.AddOption(txOption);
        cmd.AddOption(outOption);
        cmd.AddOption(srOption);
        cmd.AddOption(bitOption);
        cmd.AddOption(fadeMsOption);
        cmd.AddOption(toneDbOption);

        cmd.SetHandler(async (context) =>
        {
            var txFile = context.ParseResult.GetValueForOption(txOption)!;
            var outWav = context.ParseResult.GetValueForOption(outOption)!;
            var sr = context.ParseResult.GetValueForOption(srOption);
            var bit = context.ParseResult.GetValueForOption(bitOption);
            var fadeMs = context.ParseResult.GetValueForOption(fadeMsOption);
            var toneDb = context.ParseResult.GetValueForOption(toneDbOption);

            try
            {
                await RunRenderAsync(txFile, outWav, sr, bit, fadeMs, toneDb);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        });

        return cmd;
    }

    private static async Task RunRenderAsync(FileInfo txFile, FileInfo outWav, int sampleRate, int bitDepth, double fadeMs, double toneDb)
    {
        if (!txFile.Exists) throw new FileNotFoundException($"TranscriptIndex not found: {txFile.FullName}");

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var txJson = await File.ReadAllTextAsync(txFile.FullName);
        var tx = JsonSerializer.Deserialize<Ams.Core.Align.Tx.TranscriptIndex>(txJson, jsonOptions) ?? throw new InvalidOperationException("Failed to parse TranscriptIndex");

        string audioPath = NormalizePath(tx.AudioPath);
        string asrPath = NormalizePath(tx.ScriptPath);
        if (!File.Exists(audioPath)) throw new FileNotFoundException($"Audio file not found: {audioPath}");
        if (!File.Exists(asrPath)) throw new FileNotFoundException($"ASR file not found: {asrPath}");

        var asrJson = await File.ReadAllTextAsync(asrPath);
        var asr = JsonSerializer.Deserialize<AsrResponse>(asrJson, jsonOptions) ?? throw new InvalidOperationException("Failed to parse ASR JSON referenced by TranscriptIndex");

        Console.WriteLine($"Reading WAV: {audioPath}");
        var wav = WavIo.ReadPcmOrFloat(audioPath);

        Console.WriteLine($"Rendering roomtone with sentence masks and {fadeMs} ms crossfades...");
        var rendered = RoomtoneRenderer.RenderWithSentenceMasks(
            input: wav,
            asr: asr,
            sentences: tx.Sentences,
            targetSampleRate: sampleRate,
            toneGainDb: toneDb,
            fadeMs: fadeMs);

        if (bitDepth != 16)
            throw new NotSupportedException("Currently only 16-bit PCM output is supported in MVP.");

        Console.WriteLine($"Writing 16-bit PCM WAV @ {sampleRate} Hz: {outWav.FullName}");
        EnsureDirectory(outWav.DirectoryName);
        WavIo.WriteInt16Pcm(outWav.FullName, rendered);
        Console.WriteLine("Done.");
    }

    private static void EnsureDirectory(string? dir)
    {
        if (string.IsNullOrWhiteSpace(dir)) return;
        Directory.CreateDirectory(dir);
    }

    private static string NormalizePath(string path)
    {
        // Accept Windows paths (e.g., C:\foo\bar.wav) when running on Linux/WSL by mapping to /mnt/c/...
        if (OperatingSystem.IsWindows()) return path;
        if (path.Length >= 3 && char.IsLetter(path[0]) && path[1] == ':' && (path[2] == '\\' || path[2] == '/'))
        {
            var drive = char.ToLowerInvariant(path[0]);
            var rest = path.Substring(2).Replace('\\', '/');
            return $"/mnt/{drive}{rest}";
        }
        return path;
    }
}
