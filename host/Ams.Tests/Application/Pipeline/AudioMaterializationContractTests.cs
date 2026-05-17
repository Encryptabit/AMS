using Xunit.Sdk;

namespace Ams.Tests.Application.Pipeline;

public sealed class AudioMaterializationContractTests
{
    private const string AsrServiceRelativePath = "host/Ams.Core/Services/AsrService.cs";
    private const string AsrProcessorRelativePath = "host/Ams.Core/Processors/AsrProcessor.cs";
    private const string MfaWorkflowRelativePath = "host/Ams.Core/Application/Mfa/MfaWorkflow.cs";

    [Fact]
    public void AsrService_ChunkedTranscription_UsesBufferForWhisperNetAndPreparedWavForWhisperX()
    {
        var source = ReadRepoFile(AsrServiceRelativePath);

        AssertContains(
            source,
            AsrServiceRelativePath,
            "options.Engine == AsrEngine.WhisperX",
            "chunk ASR branches only WhisperX to the file-backed transcription path");

        AssertContains(
            source,
            AsrServiceRelativePath,
            "AsrProcessor.TranscribePreparedWavFileAsync(",
            "WhisperX chunk ASR uses prepared chunk WAV artifacts");

        AssertContains(
            source,
            AsrServiceRelativePath,
            "AsrProcessor.TranscribeBufferAsync(\n                        chunkSlices[i],",
            "Whisper.NET chunk ASR keeps the already-materialized chunk slice in memory");

        AssertContains(
            source,
            AsrServiceRelativePath,
            "ASR stream chunk: state=start",
            "chunk ASR emits per-chunk streaming diagnostics");

        AssertContains(
            source,
            AsrServiceRelativePath,
            "ASR stream chunk: state=complete",
            "chunk ASR emits per-chunk completion diagnostics");

        AssertContains(
            source,
            AsrServiceRelativePath,
            "source={Source}",
            "chunk ASR diagnostics expose whether the stream came from buffer or file");
    }

    [Fact]
    public void AsrProcessor_PreparedWavPath_StreamsExistingFiles()
    {
        var source = ReadRepoFile(AsrProcessorRelativePath);

        AssertContains(
            source,
            AsrProcessorRelativePath,
            "public static Task<AsrResponse> TranscribePreparedWavFileAsync(",
            "explicit prepared-WAV transcription entry point");

        AssertContains(
            source,
            AsrProcessorRelativePath,
            "new FileStream(\n                    audioPath,",
            "Whisper.NET prepared-WAV path opens the existing file stream");

        AssertContains(
            source,
            AsrProcessorRelativePath,
            "RunWhisperXPreparedWavFilePassAsync(audioPath, options, cancellationToken)",
            "WhisperX prepared-WAV path passes the existing file path to the process runner");
    }

    [Fact]
    public void MfaWorkflow_ChunkedCorpus_PassesLazyAudioDecodeFactory()
    {
        var source = ReadRepoFile(MfaWorkflowRelativePath);

        AssertContains(
            source,
            MfaWorkflowRelativePath,
            "audioBufferFactory: () => AudioProcessor.Decode(audioFile.FullName)",
            "chunked MFA corpus receives lazy audio decode factory");

        AssertDoesNotContain(
            source,
            MfaWorkflowRelativePath,
            "var audioBuffer = AudioProcessor.Decode(audioFile.FullName);",
            "chunked MFA corpus must not decode full chapter audio before reusable chunk audio is checked");
    }

    [Fact]
    public void MfaChunkCorpusBuilder_LogsChunkAudioReuseAndFallbackDecisions()
    {
        var source = ReadRepoFile("host/Ams.Core/Application/Mfa/MfaChunkCorpusBuilder.cs");

        AssertContains(
            source,
            "host/Ams.Core/Application/Mfa/MfaChunkCorpusBuilder.cs",
            "MFA chunk audio: action=reuse",
            "MFA corpus logs successful ASR chunk audio reuse");

        AssertContains(
            source,
            "host/Ams.Core/Application/Mfa/MfaChunkCorpusBuilder.cs",
            "MFA chunk audio: action=fallback-slice",
            "MFA corpus logs fallback from chunk WAV reuse to buffer slicing");

        AssertContains(
            source,
            "host/Ams.Core/Application/Mfa/MfaChunkCorpusBuilder.cs",
            "MFA chunk audio: action=load-buffer",
            "MFA corpus logs the first lazy full-buffer load");
    }

    private static void AssertContains(string source, string relativePath, string anchor, string description)
    {
        Assert.True(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Missing audio materialization contract anchor '{description}' in '{relativePath}'. Expected snippet: {anchor}");
    }

    private static void AssertDoesNotContain(string source, string relativePath, string anchor, string description)
    {
        Assert.False(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Found stale audio materialization contract anchor '{description}' in '{relativePath}'. Remove snippet: {anchor}");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var repoRoot = FindRepoRoot();
        var fullPath = Path.Combine(repoRoot, relativePath);

        if (!File.Exists(fullPath))
        {
            throw new XunitException($"Required audio materialization source file is missing: relative='{relativePath}', full='{fullPath}'.");
        }

        return File.ReadAllText(fullPath);
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "CODE-STYLE.md"))
                && Directory.Exists(Path.Combine(current.FullName, "host"))
                && Directory.Exists(Path.Combine(current.FullName, ".gsd")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repo root containing CODE-STYLE.md, host/, and .gsd/.");
    }
}
