using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core.Io;

namespace Ams.Core.Pipeline;

public class AlignChunksStage : StageRunner
{
    private readonly HttpClient _httpClient;
    private readonly AlignmentParams _params;

    public AlignChunksStage(
        string workDir,
        HttpClient httpClient,
        AlignmentParams parameters)
        : base(workDir, "align-chunks")
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    protected override async Task<Dictionary<string, string>> RunStageAsync(ManifestV2 manifest, string stageDir, CancellationToken ct)
    {
        // Load chunk index
        var chunkIndexPath = Path.Combine(WorkDir, "chunks", "index.json");
        if (!File.Exists(chunkIndexPath))
            throw new InvalidOperationException("Chunk index not found. Run 'chunks' stage first.");

        // Load transcript index
        var transcriptIndexPath = Path.Combine(WorkDir, "transcripts", "index.json");
        if (!File.Exists(transcriptIndexPath))
            throw new InvalidOperationException("Transcript index not found. Run 'transcripts' stage first.");

        var chunkIndexJson = await File.ReadAllTextAsync(chunkIndexPath, ct);
        var chunkIndex = JsonSerializer.Deserialize<ChunkIndex>(chunkIndexJson) ?? throw new InvalidOperationException("Invalid chunk index");

        var transcriptIndexJson = await File.ReadAllTextAsync(transcriptIndexPath, ct);
        var transcriptIndex = JsonSerializer.Deserialize<TranscriptIndex>(transcriptIndexJson) ?? throw new InvalidOperationException("Invalid transcript index");

        Console.WriteLine($"Aligning {chunkIndex.Chunks.Count} chunks using Aeneas service...");

        var alignDir = Path.Combine(stageDir, "chunks");
        Directory.CreateDirectory(alignDir);

        foreach (var chunk in chunkIndex.Chunks)
        {
            Console.WriteLine($"Aligning chunk {chunk.Id}...");

            var alignment = await AlignChunkAsync(chunk, transcriptIndex, ct);

            var outputPath = Path.Combine(alignDir, $"{chunk.Id}.aeneas.json");
            var alignmentJson = JsonSerializer.Serialize(alignment, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(outputPath, alignmentJson, ct);

            Console.WriteLine($"Chunk {chunk.Id}: {alignment.Fragments.Count} fragments aligned");
        }

        var paramsPath = Path.Combine(stageDir, "params.snapshot.json");
        var paramsJson = SerializeParams(_params);
        await File.WriteAllTextAsync(paramsPath, paramsJson, ct);

        Console.WriteLine($"Aligned {chunkIndex.Chunks.Count} chunks");

        return new Dictionary<string, string>
        {
            ["chunks_dir"] = "chunks",
            ["params"] = "params.snapshot.json"
        };
    }

    protected override async Task<StageFingerprint> ComputeFingerprintAsync(ManifestV2 manifest, CancellationToken ct)
    {
        var paramsHash = ComputeHash(SerializeParams(_params));

        // Include both chunk and transcript hashes in input hash
        var chunkIndexPath = Path.Combine(WorkDir, "chunks", "index.json");
        var transcriptIndexPath = Path.Combine(WorkDir, "transcripts", "index.json");

        var chunkHash = "";
        var transcriptHash = "";

        if (File.Exists(chunkIndexPath))
        {
            var chunkContent = await File.ReadAllTextAsync(chunkIndexPath, ct);
            chunkHash = ComputeHash(chunkContent);
        }

        if (File.Exists(transcriptIndexPath))
        {
            var transcriptContent = await File.ReadAllTextAsync(transcriptIndexPath, ct);
            transcriptHash = ComputeHash(transcriptContent);
        }

        var inputHash = ComputeHash(manifest.Input.Sha256 + chunkHash + transcriptHash);
        var toolVersions = await GetToolVersionsAsync(ct);

        return new StageFingerprint(inputHash, paramsHash, toolVersions);
    }

    private async Task<ChunkAlignment> AlignChunkAsync(ChunkInfo chunk, TranscriptIndex transcriptIndex, CancellationToken ct)
    {
        // Get transcript for this chunk
        if (!transcriptIndex.ChunkToJsonMap.TryGetValue(chunk.Id, out var transcriptFile))
            throw new InvalidOperationException($"Transcript not found for chunk {chunk.Id}");

        var transcriptPath = Path.Combine(WorkDir, "transcripts", "raw", transcriptFile);
        var transcriptJson = await File.ReadAllTextAsync(transcriptPath, ct);
        var transcript = JsonSerializer.Deserialize<ChunkTranscript>(transcriptJson) ?? throw new InvalidOperationException("Invalid transcript");

        // Extract sentences/lines from transcript text for alignment
        var lines = ExtractLinesFromTranscript(transcript);

        if (lines.Count == 0)
        {
            Console.WriteLine($"Warning: No text lines found for chunk {chunk.Id}, creating empty alignment");
            return new ChunkAlignment(
                chunk.Id,
                chunk.Span.Start,
                _params.Language,
                ComputeTextDigest(lines),
                new List<AlignmentFragment>(),
                new Dictionary<string, string>(),
                DateTime.UtcNow
            );
        }

        // Get chunk audio path
        var chunkPath = Path.Combine(WorkDir, "chunks", "wav", chunk.Filename);
        var normalizedChunkPath = PathNormalizer.NormalizePath(chunkPath);

        if (!File.Exists(normalizedChunkPath))
            throw new FileNotFoundException($"Chunk audio file not found: {normalizedChunkPath}");

        // Call Aeneas service
        var requestBody = new
        {
            chunk_id = chunk.Id,
            audio_path = normalizedChunkPath,
            lines = lines,
            language = _params.Language,
            timeout_sec = _params.TimeoutSec
        };

        var requestJson = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_params.ServiceUrl}/v1/align-chunk", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Aeneas alignment service error ({response.StatusCode}): {errorContent}");
        }

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var alignmentResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);

        // Parse alignment response
        var fragments = new List<AlignmentFragment>();
        if (alignmentResponse.TryGetProperty("fragments", out var fragmentsArray))
        {
            foreach (var fragmentElement in fragmentsArray.EnumerateArray())
            {
                var fragment = new AlignmentFragment(
                    fragmentElement.GetProperty("begin").GetDouble(),
                    fragmentElement.GetProperty("end").GetDouble()
                );
                fragments.Add(fragment);
            }
        }

        var toolVersions = new Dictionary<string, string>();
        if (alignmentResponse.TryGetProperty("tool", out var toolElement))
        {
            if (toolElement.TryGetProperty("python", out var python))
                toolVersions["python"] = python.GetString() ?? "unknown";
            if (toolElement.TryGetProperty("aeneas", out var aeneas))
                toolVersions["aeneas"] = aeneas.GetString() ?? "unknown";
        }

        return new ChunkAlignment(
            chunk.Id,
            chunk.Span.Start, // Offset for converting to chapter time later
            _params.Language,
            ComputeTextDigest(lines),
            fragments,
            toolVersions,
            DateTime.UtcNow
        );
    }

    private List<string> ExtractLinesFromTranscript(ChunkTranscript transcript)
    {
        // For now, use the full transcript text as a single line
        // In the future, this could be enhanced to split into sentences
        var lines = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(transcript.Text))
        {
            // Split by sentence-ending punctuation for better alignment
            var sentences = transcript.Text
                .Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            if (sentences.Count > 0)
            {
                lines.AddRange(sentences);
            }
            else
            {
                // Fallback to full text
                lines.Add(transcript.Text.Trim());
            }
        }

        return lines;
    }

    private static string ComputeTextDigest(List<string> lines)
    {
        var combined = string.Join('\n', lines).Trim();
        return ComputeHash(combined)[..16]; // First 16 chars for brevity
    }

    private async Task<Dictionary<string, string>> GetToolVersionsAsync(CancellationToken ct)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{_params.ServiceUrl}/v1/health", null, ct);
            if (response.IsSuccessStatusCode)
            {
                var healthJson = await response.Content.ReadAsStringAsync(ct);
                var health = JsonSerializer.Deserialize<JsonElement>(healthJson);

                var versions = new Dictionary<string, string>();
                if (health.TryGetProperty("python_version", out var python))
                    versions["python"] = python.GetString() ?? "unknown";
                if (health.TryGetProperty("aeneas_version", out var aeneas))
                    versions["aeneas"] = aeneas.GetString() ?? "unknown";

                return versions;
            }
        }
        catch
        {
            // Ignore health check failures for fingerprinting
        }

        return new Dictionary<string, string>
        {
            ["python"] = "unknown",
            ["aeneas"] = "unknown"
        };
    }
}