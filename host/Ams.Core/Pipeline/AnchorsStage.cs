using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Ams.Align.Anchors;

namespace Ams.Core.Pipeline;

public class AnchorsStage : StageRunner
{
    private readonly AnchorsParams _params;
    private readonly string _bookIndexPath;
    private readonly string _asrPath;

    public AnchorsStage(
        string workDir,
        AnchorsParams parameters,
        string bookIndexPath,
        string asrPath)
        : base(workDir, "anchors")
    {
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
        _bookIndexPath = bookIndexPath ?? throw new ArgumentNullException(nameof(bookIndexPath));
        _asrPath = asrPath ?? throw new ArgumentNullException(nameof(asrPath));
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        Console.WriteLine($"Mining n-gram anchors (n={_params.Ngram}, min-sep={_params.MinSeparation})...");

        // Load BookIndex
        var bookIndexJson = await File.ReadAllTextAsync(_bookIndexPath, ct);
        var book = JsonSerializer.Deserialize<BookIndex>(bookIndexJson) ?? throw new InvalidOperationException("Invalid BookIndex JSON");

        // Build normalized book tokens and sentence indices
        var bookTokens = new List<string>(book.Words.Length);
        var bookSentenceIndex = new List<int>(book.Words.Length);
        foreach (var w in book.Words)
        {
            var tok = AnchorTokenizer.Normalize(w.Text);
            if (tok.Length == 0) tok = string.Empty; // keep position alignment
            bookTokens.Add(tok);
            bookSentenceIndex.Add(w.SentenceIndex);
        }

        // Load ASR tokens from provided JSON (supports multiple shapes)
        var asrTokens = await LoadAsrTokensAsync(_asrPath, ct);

        // Build stopword set
        ISet<string> stop = _params.Stopwords?.Equals("en-basic", StringComparison.OrdinalIgnoreCase) == true
            ? StopwordSets.EnglishPlusDomain
            : StopwordSets.EnglishPlusDomain; // default for now

        // Translate TargetPerTokens ratio (e.g., 0.02) → AnchorPolicy.TargetPerTokens (e.g., 50)
        int targetPerTokens = _params.TargetPerTokens <= 0.0
            ? 50
            : Math.Max(1, (int)Math.Round(1.0 / _params.TargetPerTokens));

        var policy = new AnchorPolicy(
            NGram: Math.Max(2, _params.Ngram),
            TargetPerTokens: targetPerTokens,
            AllowDuplicates: false,
            MinSeparation: Math.Max(1, _params.MinSeparation),
            Stopwords: new HashSet<string>(stop, StringComparer.Ordinal),
            DisallowBoundaryCross: true
        );

        // Run discovery (deterministic)
        var selected = AnchorDiscovery.SelectAnchors(bookTokens, bookSentenceIndex, asrTokens, policy).ToList();

        // Ensure synthetic start anchor exists at (0,0) if not already present
        if (selected.Count == 0 || selected[0].Bp != 0 || selected[0].Ap != 0)
        {
            selected.Insert(0, new Anchor(0, 0));
        }

        // Prepare result objects (Candidates omitted for now to keep file size small)
        var chosen = new List<AnchorCandidate>(selected.Count);
        foreach (var a in selected)
        {
            string ngramStr = BuildNGram(bookTokens, a.Bp, Math.Max(2, _params.Ngram));
            chosen.Add(new AnchorCandidate(ngramStr, a.Bp, a.Ap, Score: 1.0, IsSelected: true));
        }

        var meta = new Dictionary<string, string>
        {
            ["bookDigest"] = Sha256Hex(string.Join(" ", bookTokens)),
            ["asrDigest"] = Sha256Hex(string.Join(" ", asrTokens))
        };

        var tokensMeta = new Dictionary<string, int> { ["book"] = bookTokens.Count, ["asr"] = asrTokens.Count };

        var stats = new Dictionary<string, object>
        {
            ["anchorCount"] = chosen.Count,
            ["anchorDensityPer1k"] = bookTokens.Count == 0 ? 0.0 : (double)chosen.Count / (bookTokens.Count / 1000.0),
            ["relaxationsUsed"] = 0,
            ["monotoneViolations"] = 0
        };

        var anchorsResult = new AnchorsResult(
            Meta: meta,
            Params: _params,
            Tokens: tokensMeta,
            Candidates: new List<AnchorCandidate>(),
            Selected: chosen,
            Stats: stats
        );

        Directory.CreateDirectory(stageDir);
        var anchorsPath = Path.Combine(stageDir, "anchors.v2.json");
        var json = JsonSerializer.Serialize(anchorsResult, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(anchorsPath, json, ct);

        var paramsPath = Path.Combine(stageDir, "params.snapshot.json");
        var paramsJson = SerializeParams(_params);
        await File.WriteAllTextAsync(paramsPath, paramsJson, ct);

        Console.WriteLine($"Selected {anchorsResult.Selected.Count} anchors");

        return new Dictionary<string, string>
        {
            ["anchors"] = "anchors.v2.json",
            ["params"] = "params.snapshot.json"
        };
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        // Compute fingerprint based on BookIndex + ASR tokens + params + tool versions
        var paramsHash = ComputeHash(SerializeParams(_params));

        string bookHash;
        using (var sha = SHA256.Create())
        {
            var text = await File.ReadAllTextAsync(_bookIndexPath, ct);
            bookHash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(text)));
        }

        string asrHash;
        using (var sha = SHA256.Create())
        {
            var text = await File.ReadAllTextAsync(_asrPath, ct);
            asrHash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(text)));
        }

        var inputHash = ComputeHash(bookHash + "\n" + asrHash);

        var toolVersions = new Dictionary<string, string>
        {
            ["tokenizer"] = "1.0.0",
            ["stopwords"] = _params.Stopwords
        };

        return new StageFingerprint(
            inputHash,
            paramsHash,
            toolVersions
        );
    }

    private static async Task<List<string>> LoadAsrTokensAsync(string path, CancellationToken ct)
    {
        var text = await File.ReadAllTextAsync(path, ct);
        try
        {
            var node = JsonNode.Parse(text);
            if (node is JsonObject obj)
            {
                // Case 1: transcripts index with chunk map
                if (obj.ContainsKey("ChunkToJsonMap") && obj["ChunkToJsonMap"] is JsonObject map)
                {
                    var toks = new List<string>();
                    foreach (var kv in map)
                    {
                        var chunkPath = kv.Value?.GetValue<string>();
                        if (string.IsNullOrWhiteSpace(chunkPath) || !File.Exists(chunkPath!)) continue;
                        var cj = await File.ReadAllTextAsync(chunkPath!, ct);
                        var chunk = JsonSerializer.Deserialize<ChunkTranscript>(cj);
                        if (chunk?.Words != null)
                        {
                            foreach (var w in chunk.Words)
                            {
                                var t = AnchorTokenizer.Normalize(w.Word);
                                if (t.Length > 0) toks.Add(t);
                            }
                        }
                    }
                    return toks;
                }

                // Case 2: object with words array of objects
                if (obj.ContainsKey("words") && obj["words"] is JsonArray arr)
                {
                    var toks = new List<string>();
                    foreach (var el in arr)
                    {
                        if (el is JsonObject ow)
                        {
                            var word = ow["word"]?.GetValue<string>() ?? ow["Word"]?.GetValue<string>();
                            if (!string.IsNullOrEmpty(word))
                            {
                                var t = AnchorTokenizer.Normalize(word);
                                if (t.Length > 0) toks.Add(t);
                            }
                        }
                        else if (el is JsonValue sv && sv.TryGetValue<string>(out var s))
                        {
                            var t = AnchorTokenizer.Normalize(s);
                            if (t.Length > 0) toks.Add(t);
                        }
                    }
                    return toks;
                }

                // Case 3: object with text
                if (obj.ContainsKey("text"))
                {
                    var s = obj["text"]!.GetValue<string>();
                    return TokenizeText(s);
                }
            }
        }
        catch
        {
            // Fall back to plain-text handling
        }

        // Fallback: treat file as plain text
        return TokenizeText(text);
    }

    private static List<string> TokenizeText(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return new List<string>();
        var parts = s.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var toks = new List<string>(parts.Length);
        foreach (var p in parts)
        {
            var t = AnchorTokenizer.Normalize(p);
            if (t.Length > 0) toks.Add(t);
        }
        return toks;
    }

    private static string BuildNGram(IReadOnlyList<string> tokens, int bp, int n)
    {
        if (bp < 0 || bp >= tokens.Count) return string.Empty;
        int len = Math.Min(n, Math.Max(0, tokens.Count - bp));
        return string.Join("|", tokens.Skip(bp).Take(len));
    }

    private static string Sha256Hex(string s)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(s)));
    }
}
