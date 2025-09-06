using System.Text.Json;
using Ams.Align.Anchors;

namespace Ams.Core.Pipeline;

public class AnchorsStage : StageRunner
{
    private readonly AnchorsParams _params;
    private readonly string _bookPath;
    private readonly string _asrMergedPath;

    public AnchorsStage(string workDir, string bookIndexPath, string asrMergedPath, AnchorsParams parameters)
        : base(workDir, "anchors")
    {
        _bookPath = bookIndexPath ?? throw new ArgumentNullException(nameof(bookIndexPath));
        _asrMergedPath = asrMergedPath ?? throw new ArgumentNullException(nameof(asrMergedPath));
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        Directory.CreateDirectory(stageDir);
        var bookJson = await File.ReadAllTextAsync(_bookPath, ct);
        var asrMergedJson = await File.ReadAllTextAsync(_asrMergedPath, ct);

        var book = JsonSerializer.Deserialize<BookIndex>(bookJson)
                   ?? throw new InvalidOperationException("Invalid BookIndex JSON.");

        var merged = JsonSerializer.Deserialize<JsonElement>(asrMergedJson);
        var tokens = new List<AsrToken>();
        if (merged.TryGetProperty("Words", out var wordsArr))
        {
            foreach (var w in wordsArr.EnumerateArray())
            {
                var txt = w.GetProperty("Word").GetString() ?? string.Empty;
                var start = w.GetProperty("Start").GetDouble();
                var end = w.GetProperty("End").GetDouble();
                tokens.Add(new AsrToken(start, Math.Max(0.0, end - start), txt));
            }
        }
        else
        {
            throw new InvalidOperationException("transcripts/merged.json missing Words[]");
        }

        var asr = new AsrResponse("merged/derived", tokens.ToArray());

        var policy = new AnchorPolicy(
            NGram: _params.NGram,
            TargetPerTokens: Math.Max(1, _params.TargetPerTokens),
            AllowDuplicates: false,
            MinSeparation: _params.MinSeparation,
            Stopwords: StopwordSets.EnglishPlusDomain,
            DisallowBoundaryCross: true
        );

        var result = AnchorPipeline.ComputeAnchors(book, asr, policy, new SectionDetectOptions(true, 8), includeWindows: false);

        var selected = result.Anchors.Select(a => new AnchorSelection(a.Bp, a.Ap, _params.NGram)).ToList();
        var artifact = new AnchorsArtifact(
            BookSha256: book.SourceFileHash,
            AsrMergedSha256: ComputeHash(asrMergedJson),
            Params: _params,
            BookTokenCount: result.BookFilteredCount,
            AsrTokenCount: result.AsrFilteredCount,
            Selected: selected,
            Candidates: null,
            Stats: new { result.SectionDetected, Section = result.Section?.Title, result.BookWindowFiltered },
            ToolVersions: new Dictionary<string, string>()
        );

        var outPath = Path.Combine(stageDir, "anchors.json");
        await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(artifact, new JsonSerializerOptions { WriteIndented = true }), ct);

        var paramsPath = Path.Combine(stageDir, "params.snapshot.json");
        await File.WriteAllTextAsync(paramsPath, SerializeParams(_params), ct);

        return new Dictionary<string, string> {
            ["anchors"] = "anchors.json",
            ["params"] = "params.snapshot.json"
        };
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        var paramsHash = ComputeHash(SerializeParams(_params));
        var bookHash = ComputeHash(await File.ReadAllTextAsync(_bookPath, ct));
        var asrHash = ComputeHash(await File.ReadAllTextAsync(_asrMergedPath, ct));
        var inputHash = ComputeHash(bookHash + "\n" + asrHash);
        return new StageFingerprint(inputHash, paramsHash, new Dictionary<string, string>());
    }
}
