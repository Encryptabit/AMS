using System.CommandLine;
using System.Text.Json;
using Ams.Core.Align.Tx;
using Ams.Core;

namespace Ams.Cli.Commands;

public static class RefineSentencesCommand
{
    public static Command Create()
    {
        var cmd = new Command("refine-sentences", "Refine sentence start/end times: start from Aeneas, end from FFmpeg silence");

        var txOption = new Option<FileInfo>("--tx-json", description: "TranscriptIndex JSON") { IsRequired = true };
        txOption.AddAlias("-t");
        var asrOption = new Option<FileInfo>("--asr-json", description: "ASR JSON used by TX") { IsRequired = true };
        asrOption.AddAlias("-j");
        var audioOption = new Option<FileInfo>("--audio", description: "Audio WAV") { IsRequired = true };
        audioOption.AddAlias("-a");
        var outOption = new Option<FileInfo>("--out", description: "Output refined sentence JSON") { IsRequired = true };
        outOption.AddAlias("-o");
        var langOption = new Option<string>("--language", () => "eng", "Aeneas language code");
        var useSilenceOption = new Option<bool>("--with-silence", () => true, "Use FFmpeg silencedetect to refine sentence ends");
        var silenceThreshOption = new Option<double>("--silence-threshold-db", () => -30.0, "Silence threshold in dBFS (e.g., -30)");
        silenceThreshOption.AddAlias("--db-floor");
        var silenceMinDurOption = new Option<double>("--silence-min-dur", () => 0.1, "Minimum silence duration in seconds");
        silenceMinDurOption.AddAlias("--min-dur");

        cmd.AddOption(txOption);
        cmd.AddOption(asrOption);
        cmd.AddOption(audioOption);
        cmd.AddOption(outOption);
        cmd.AddOption(langOption);
        cmd.AddOption(useSilenceOption);
        cmd.AddOption(silenceThreshOption);
        cmd.AddOption(silenceMinDurOption);

        cmd.SetHandler(async (context) =>
        {
            var txFile = context.ParseResult.GetValueForOption(txOption)!;
            var asrFile = context.ParseResult.GetValueForOption(asrOption)!;
            var audioFile = context.ParseResult.GetValueForOption(audioOption)!;
            var outFile = context.ParseResult.GetValueForOption(outOption)!;
            var lang = context.ParseResult.GetValueForOption(langOption)!;
            var withSilence = context.ParseResult.GetValueForOption(useSilenceOption);
            var silenceDb = context.ParseResult.GetValueForOption(silenceThreshOption);
            var silenceMin = context.ParseResult.GetValueForOption(silenceMinDurOption);

            try
            {
                await RunAsync(txFile, asrFile, audioFile, outFile, lang, withSilence, silenceDb, silenceMin);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        });

        return cmd;
    }

    private static async Task RunAsync(FileInfo txFile, FileInfo asrFile, FileInfo audioFile, FileInfo outFile, string language, bool withSilence, double silenceDb, double silenceMin)
    {
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var tx = JsonSerializer.Deserialize<Ams.Core.Align.Tx.TranscriptIndex>(await File.ReadAllTextAsync(txFile.FullName), jsonOptions)
                 ?? throw new InvalidOperationException("Failed to read TX");
        var asr = JsonSerializer.Deserialize<AsrResponse>(await File.ReadAllTextAsync(asrFile.FullName), jsonOptions)
                 ?? throw new InvalidOperationException("Failed to read ASR");

        var svc = new SentenceRefinementService();
        var refined = await svc.RefineAsync(audioFile.FullName, tx, asr, language, withSilence, silenceDb, silenceMin);

        var outJson = JsonSerializer.Serialize(refined, new JsonSerializerOptions { WriteIndented = true });
        Directory.CreateDirectory(outFile.DirectoryName!);
        await File.WriteAllTextAsync(outFile.FullName, outJson);
        Console.WriteLine($"Refined sentences written: {outFile.FullName}");
    }
}
