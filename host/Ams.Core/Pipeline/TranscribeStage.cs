using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Io;

namespace Ams.Core.Pipeline;

public class TranscribeStage : StageRunner
{
    private readonly HttpClient _httpClient;
    private readonly TranscriptionParams _params;

    public TranscribeStage(
        string workDir,
        HttpClient httpClient,
        TranscriptionParams parameters)
        : base(workDir, "transcripts")
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        var indexPath = Path.Combine(WorkDir, "chunks", "index.json");
        if (!File.Exists(indexPath))
            throw new InvalidOperationException("Chunk index not found. Run 'chunks' stage first.");

        var indexJson = await File.ReadAllTextAsync(indexPath, ct);
        var chunkIndex = JsonSerializer.Deserialize<ChunkIndex>(indexJson) ?? throw new InvalidOperationException("Invalid chunk index");

        Console.WriteLine($"Transcribing {chunkIndex.Chunks.Count} chunks using {_params.Model}...");

        var rawDir = Path.Combine(stageDir, "raw");
        Directory.CreateDirectory(rawDir);

        var chunkToJsonMap = new Dictionary<string, string>();
        var mergedWords = new List<TranscriptWord>();

        foreach (var chunk in chunkIndex.Chunks)
        {
            Console.WriteLine($"Transcribing chunk {chunk.Id}...");

            var chunkPath = Path.Combine(WorkDir, "chunks", "wav", chunk.Filename);
            var transcript = await TranscribeChunkAsync(chunk, chunkPath, ct);

            var outputPath = Path.Combine(rawDir, $"{chunk.Id}.json");
            var transcriptJson = JsonSerializer.Serialize(transcript, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(outputPath, transcriptJson, ct);

            chunkToJsonMap[chunk.Id] = $"{chunk.Id}.json";

            // Add words to merged list, adjusting times to chapter coordinates
            foreach (var word in transcript.Words)
            {
                var adjustedWord = new TranscriptWord(
                    word.Word,
                    word.Start + chunk.Span.Start, // Convert to chapter time
                    word.End + chunk.Span.Start,   // Convert to chapter time
                    word.Confidence
                );
                mergedWords.Add(adjustedWord);
            }

            Console.WriteLine($"Chunk {chunk.Id}: {transcript.Words.Count} words transcribed");
        }

        // Create merged transcript
        var merged = new
        {
            ChunkCount = chunkIndex.Chunks.Count,
            TotalWords = mergedWords.Count,
            Words = mergedWords,
            Params = _params,
            GeneratedAt = DateTime.UtcNow
        };

        var mergedPath = Path.Combine(stageDir, "merged.json");
        var mergedJson = JsonSerializer.Serialize(merged, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(mergedPath, mergedJson, ct);

        // Create transcript index
        var toolVersions = await GetToolVersionsAsync(ct);
        var transcriptIndex = new TranscriptIndex(
            chunkIndex.Chunks.Select(c => c.Id).ToList(),
            chunkToJsonMap,
            _params,
            toolVersions
        );

        var transcriptIndexPath = Path.Combine(stageDir, "index.json");
        var indexOutputJson = JsonSerializer.Serialize(transcriptIndex, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(transcriptIndexPath, indexOutputJson, ct);

        var paramsPath = Path.Combine(stageDir, "params.snapshot.json");
        var paramsJson = SerializeParams(_params);
        await File.WriteAllTextAsync(paramsPath, paramsJson, ct);

        Console.WriteLine($"Transcribed {chunkIndex.Chunks.Count} chunks, {mergedWords.Count} total words");

        return new Dictionary<string, string>
        {
            ["index"] = "index.json",
            ["merged"] = "merged.json",
            ["raw_dir"] = "raw",
            ["params"] = "params.snapshot.json"
        };
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        var paramsHash = ComputeHash(SerializeParams(_params));

        // Include chunk index hash in input hash since transcripts depend on chunks
        var indexPath = Path.Combine(WorkDir, "chunks", "index.json");
        var indexHash = "";
        if (File.Exists(indexPath))
        {
            var indexContent = await File.ReadAllTextAsync(indexPath, ct);
            indexHash = ComputeHash(indexContent);
        }

        var inputHash = ComputeHash(manifest.Input.Sha256 + indexHash);
        var toolVersions = await GetToolVersionsAsync(ct);

        return new StageFingerprint(inputHash, paramsHash, toolVersions);
    }

    private async Task<ChunkTranscript> TranscribeChunkAsync(ChunkInfo chunk, string chunkPath, CancellationToken ct)
    {
        var normalizedPath = PathNormalizer.NormalizePath(chunkPath);

        if (!File.Exists(normalizedPath))
            throw new FileNotFoundException($"Chunk file not found: {normalizedPath}");

        // Use audio file path directly (not base64)
        var requestBody = new
        {
            audio_path = normalizedPath,
            model = _params.Model,
            language = _params.Language
        };

        var requestJson = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_params.ServiceUrl}/asr", content, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Transcription service error ({response.StatusCode}): {errorContent}");
        }

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

        var words = new List<TranscriptWord>();
        var fullText = "";

        // Handle your ASR service format: {"tokens": [{"t": start, "d": duration, "w": word}]}
        if (responseObj.TryGetProperty("tokens", out var tokensArray))
        {
            foreach (var tokenElement in tokensArray.EnumerateArray())
            {
                var wordText = tokenElement.GetProperty("w").GetString() ?? "";
                var startTime = tokenElement.GetProperty("t").GetDouble();
                var duration = tokenElement.GetProperty("d").GetDouble();
                var endTime = startTime + duration;
                
                var word = new TranscriptWord(
                    wordText,
                    startTime,
                    endTime,
                    1.0 // Default confidence since your ASR doesn't provide it
                );
                words.Add(word);
            }
            
            // Create full text by joining all words
            fullText = string.Join(" ", words.Select(w => w.Word));
        }
        else if (responseObj.TryGetProperty("words", out var wordsArray))
        {
            // Fallback: handle standard format if present
            foreach (var wordElement in wordsArray.EnumerateArray())
            {
                var word = new TranscriptWord(
                    wordElement.GetProperty("word").GetString() ?? "",
                    wordElement.GetProperty("start").GetDouble(),
                    wordElement.GetProperty("end").GetDouble(),
                    wordElement.TryGetProperty("confidence", out var conf) ? conf.GetDouble() : 1.0
                );
                words.Add(word);
            }
            
            if (responseObj.TryGetProperty("text", out var textProperty))
            {
                fullText = textProperty.GetString() ?? "";
            }
            else
            {
                fullText = string.Join(" ", words.Select(w => w.Word));
            }
        }

        var toolVersions = new Dictionary<string, string>();
        if (responseObj.TryGetProperty("tool_versions", out var toolsProperty))
        {
            foreach (var prop in toolsProperty.EnumerateObject())
            {
                toolVersions[prop.Name] = prop.Value.GetString() ?? "unknown";
            }
        }

        return new ChunkTranscript(
            chunk.Id,
            fullText,
            words,
            chunk.DurationSec,
            toolVersions,
            DateTime.UtcNow
        );
    }

    private async Task<Dictionary<string, string>> GetToolVersionsAsync(CancellationToken ct)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_params.ServiceUrl}/v1/health", ct);
            if (response.IsSuccessStatusCode)
            {
                var healthJson = await response.Content.ReadAsStringAsync(ct);
                var health = JsonSerializer.Deserialize<JsonElement>(healthJson);

                var versions = new Dictionary<string, string>();
                if (health.TryGetProperty("model_name", out var model))
                    versions["asrModel"] = model.GetString() ?? _params.Model;
                if (health.TryGetProperty("version", out var version))
                    versions["asrEngine"] = version.GetString() ?? "unknown";
                
                return versions;
            }
        }
        catch
        {
            // Ignore health check failures for fingerprinting
        }

        return new Dictionary<string, string>
        {
            ["asrModel"] = _params.Model,
            ["asrEngine"] = "unknown"
        };
    }
}