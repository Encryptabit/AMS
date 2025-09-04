using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ams.Core;
using Ams.Core.Pipeline;
using Moq;
using Moq.Protected;
using Xunit;

namespace Ams.Tests;

public class AlignChunksStageTests
{
    [Fact]
    public async Task AlignChunksStage_CallsAeneasServiceForEachChunk()
    {
        // Arrange: Mock HTTP client that returns deterministic alignment responses
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        
        // Mock health check response
        var healthResponse = new
        {
            ok = true,
            python_version = "3.9.18",
            aeneas_version = "1.7.3",
            service = "aeneas-alignment",
            timestamp = DateTime.UtcNow.ToString("O")
        };
        
        // Mock alignment responses for chunks
        var alignResponse1 = new
        {
            chunk_id = "chunk_001",
            fragments = new[]
            {
                new { begin = 0.0, end = 2.5 },
                new { begin = 2.5, end = 5.0 }
            },
            counts = new { lines = 2, fragments = 2 },
            tool = new { python = "3.9.18", aeneas = "1.7.3" },
            generated_at = DateTime.UtcNow.ToString("O")
        };
        
        var alignResponse2 = new
        {
            chunk_id = "chunk_002", 
            fragments = new[]
            {
                new { begin = 0.0, end = 3.0 }
            },
            counts = new { lines = 1, fragments = 1 },
            tool = new { python = "3.9.18", aeneas = "1.7.3" },
            generated_at = DateTime.UtcNow.ToString("O")
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", 
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/v1/health")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(healthResponse), Encoding.UTF8, "application/json")
            });

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/v1/align-chunk") && 
                                                    req.Content!.ReadAsStringAsync().Result.Contains("chunk_001")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(alignResponse1), Encoding.UTF8, "application/json")
            });

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/v1/align-chunk") && 
                                                    req.Content!.ReadAsStringAsync().Result.Contains("chunk_002")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(alignResponse2), Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        // Setup test directory structure
        using var tempDir = new TempDirectory();
        var workDir = tempDir.Path;

        // Create mock chunk index
        var chunkIndex = new ChunkIndex(
            new List<ChunkInfo>
            {
                new("chunk_001", new ChunkSpan(0.0, 10.0), "chunk_001.wav", "abc123", 10.0),
                new("chunk_002", new ChunkSpan(10.0, 18.0), "chunk_002.wav", "def456", 8.0)
            },
            "original_sha256",
            new ChunkingParams("wav", 44100)
        );
        
        var chunkIndexPath = Path.Combine(workDir, "chunks", "index.json");
        Directory.CreateDirectory(Path.GetDirectoryName(chunkIndexPath)!);
        await File.WriteAllTextAsync(chunkIndexPath, JsonSerializer.Serialize(chunkIndex));

        // Create mock transcript index
        var transcriptIndex = new TranscriptIndex(
            new List<string> { "chunk_001", "chunk_002" },
            new Dictionary<string, string> 
            { 
                ["chunk_001"] = "chunk_001.json",
                ["chunk_002"] = "chunk_002.json"
            },
            new TranscriptionParams(),
            new Dictionary<string, string>()
        );
        
        var transcriptIndexPath = Path.Combine(workDir, "transcripts", "index.json");
        Directory.CreateDirectory(Path.GetDirectoryName(transcriptIndexPath)!);
        await File.WriteAllTextAsync(transcriptIndexPath, JsonSerializer.Serialize(transcriptIndex));

        // Create mock chunk transcripts
        var transcript1 = new ChunkTranscript("chunk_001", "Hello world. This is a test.", 
            new List<TranscriptWord>(), 10.0, new Dictionary<string, string>(), DateTime.UtcNow);
        var transcript2 = new ChunkTranscript("chunk_002", "Final sentence.",
            new List<TranscriptWord>(), 8.0, new Dictionary<string, string>(), DateTime.UtcNow);

        var transcriptRawDir = Path.Combine(workDir, "transcripts", "raw");
        Directory.CreateDirectory(transcriptRawDir);
        await File.WriteAllTextAsync(Path.Combine(transcriptRawDir, "chunk_001.json"), JsonSerializer.Serialize(transcript1));
        await File.WriteAllTextAsync(Path.Combine(transcriptRawDir, "chunk_002.json"), JsonSerializer.Serialize(transcript2));

        // Create mock chunk audio files
        var chunksWavDir = Path.Combine(workDir, "chunks", "wav");
        Directory.CreateDirectory(chunksWavDir);
        await File.WriteAllTextAsync(Path.Combine(chunksWavDir, "chunk_001.wav"), "mock_audio_data");
        await File.WriteAllTextAsync(Path.Combine(chunksWavDir, "chunk_002.wav"), "mock_audio_data");

        // Create manifest
        var manifest = ManifestV2.CreateNew(new InputMetadata("test.wav", "test_sha", 18.0, 1000, DateTime.UtcNow));

        // Act: Run AlignChunksStage
        var stage = new AlignChunksStage(workDir, httpClient, new AlignmentParams("eng", 600, "http://localhost:8082"));
        var success = await stage.RunAsync(manifest, CancellationToken.None);

        // Assert: Stage should succeed
        Assert.True(success);

        // Verify alignment files were created
        var alignDir = Path.Combine(workDir, "align-chunks", "chunks");
        Assert.True(File.Exists(Path.Combine(alignDir, "chunk_001.aeneas.json")));
        Assert.True(File.Exists(Path.Combine(alignDir, "chunk_002.aeneas.json")));

        // Verify alignment content
        var alignment1Json = await File.ReadAllTextAsync(Path.Combine(alignDir, "chunk_001.aeneas.json"));
        var alignment1 = JsonSerializer.Deserialize<ChunkAlignment>(alignment1Json);
        
        Assert.NotNull(alignment1);
        Assert.Equal("chunk_001", alignment1.ChunkId);
        Assert.Equal(0.0, alignment1.OffsetSec); // First chunk starts at 0
        Assert.Equal(2, alignment1.Fragments.Count);
        Assert.Equal(0.0, alignment1.Fragments[0].Begin);
        Assert.Equal(2.5, alignment1.Fragments[0].End);

        var alignment2Json = await File.ReadAllTextAsync(Path.Combine(alignDir, "chunk_002.aeneas.json"));
        var alignment2 = JsonSerializer.Deserialize<ChunkAlignment>(alignment2Json);
        
        Assert.NotNull(alignment2);
        Assert.Equal("chunk_002", alignment2.ChunkId);
        Assert.Equal(10.0, alignment2.OffsetSec); // Second chunk starts at 10s
        Assert.Equal(1, alignment2.Fragments.Count);

        // Verify HTTP calls were made
        mockHttpMessageHandler.Protected()
            .Verify("SendAsync", Times.AtLeast(3), // Health + 2 align calls
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task AlignChunksStage_HandlesServiceErrors()
    {
        // Arrange: Mock HTTP client that returns errors
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Service unavailable")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        using var tempDir = new TempDirectory();
        var workDir = tempDir.Path;

        // Create minimal test setup
        var chunkIndex = new ChunkIndex(new List<ChunkInfo>
        {
            new("chunk_001", new ChunkSpan(0.0, 10.0), "chunk_001.wav", "abc123", 10.0)
        }, "sha", new ChunkingParams());
        
        var chunkIndexPath = Path.Combine(workDir, "chunks", "index.json");
        Directory.CreateDirectory(Path.GetDirectoryName(chunkIndexPath)!);
        await File.WriteAllTextAsync(chunkIndexPath, JsonSerializer.Serialize(chunkIndex));

        var transcriptIndex = new TranscriptIndex(
            new List<string> { "chunk_001" },
            new Dictionary<string, string> { ["chunk_001"] = "chunk_001.json" },
            new TranscriptionParams(),
            new Dictionary<string, string>()
        );
        
        var transcriptIndexPath = Path.Combine(workDir, "transcripts", "index.json");
        Directory.CreateDirectory(Path.GetDirectoryName(transcriptIndexPath)!);
        await File.WriteAllTextAsync(transcriptIndexPath, JsonSerializer.Serialize(transcriptIndex));

        var transcript = new ChunkTranscript("chunk_001", "Test text", new List<TranscriptWord>(), 10.0, new Dictionary<string, string>(), DateTime.UtcNow);
        var transcriptRawDir = Path.Combine(workDir, "transcripts", "raw");
        Directory.CreateDirectory(transcriptRawDir);
        await File.WriteAllTextAsync(Path.Combine(transcriptRawDir, "chunk_001.json"), JsonSerializer.Serialize(transcript));

        var chunksWavDir = Path.Combine(workDir, "chunks", "wav");
        Directory.CreateDirectory(chunksWavDir);
        await File.WriteAllTextAsync(Path.Combine(chunksWavDir, "chunk_001.wav"), "mock_audio_data");

        var manifest = ManifestV2.CreateNew(new InputMetadata("test.wav", "test_sha", 10.0, 1000, DateTime.UtcNow));

        // Act & Assert: Stage should fail due to service error
        var stage = new AlignChunksStage(workDir, httpClient, new AlignmentParams("eng", 600, "http://localhost:8082"));
        var success = await stage.RunAsync(manifest, CancellationToken.None);
        
        Assert.False(success);
    }

    [Fact]
    public async Task AlignChunksStage_FingerprintIncludesAllInputs()
    {
        // Arrange: Mock HTTP client for fingerprint computation
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var healthResponse = new { ok = true, python_version = "3.9.18", aeneas_version = "1.7.3" };
        
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/v1/health")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(healthResponse), Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);

        using var tempDir = new TempDirectory();
        var workDir = tempDir.Path;

        // Create test files
        Directory.CreateDirectory(Path.Combine(workDir, "chunks"));
        Directory.CreateDirectory(Path.Combine(workDir, "transcripts"));
        
        await File.WriteAllTextAsync(Path.Combine(workDir, "chunks", "index.json"), JsonSerializer.Serialize(new ChunkIndex(new List<ChunkInfo>(), "sha1", new ChunkingParams())));
        await File.WriteAllTextAsync(Path.Combine(workDir, "transcripts", "index.json"), JsonSerializer.Serialize(new TranscriptIndex(new List<string>(), new Dictionary<string, string>(), new TranscriptionParams(), new Dictionary<string, string>())));

        var manifest1 = ManifestV2.CreateNew(new InputMetadata("test.wav", "sha1", 10.0, 1000, DateTime.UtcNow));
        var manifest2 = ManifestV2.CreateNew(new InputMetadata("test.wav", "sha2", 10.0, 1000, DateTime.UtcNow)); // Different input hash

        var stage = new AlignChunksStage(workDir, httpClient, new AlignmentParams("eng", 600, "http://localhost:8082"));

        // Act: Compute fingerprints
        var fingerprint1 = await CallComputeFingerprintAsync(stage, manifest1);
        var fingerprint2 = await CallComputeFingerprintAsync(stage, manifest2);

        // Assert: Different input hashes should produce different fingerprints
        Assert.NotEqual(fingerprint1.InputHash, fingerprint2.InputHash);
    }

    // Helper methods

    private async Task<StageFingerprint> CallComputeFingerprintAsync(AlignChunksStage stage, ManifestV2 manifest)
    {
        // Use reflection to call the protected ComputeFingerprintAsync method
        var method = typeof(AlignChunksStage).GetMethod("ComputeFingerprintAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var task = (Task<StageFingerprint>)method!.Invoke(stage, new object[] { manifest, CancellationToken.None })!;
        return await task;
    }
}

// Helper class for temporary directories
public class TempDirectory : IDisposable
{
    public string Path { get; }

    public TempDirectory()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
        Directory.CreateDirectory(Path);
    }

    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}