---
phase: quick
plan: 8
type: execute
wave: 1
depends_on: []
files_modified:
  - host/Ams.Core/Audio/QualityControl/AudioQcAnalyzer.cs
  - host/Ams.Core/Audio/QualityControl/AudioQcModels.cs
  - host/Ams.Cli/Commands/QcCommand.cs
  - host/Ams.Cli/Program.cs
  - host/Ams.Tests/Audio/QualityControl/AudioQcAnalyzerTests.cs
autonomous: true
requirements: [QC-01]

must_haves:
  truths:
    - "User can run `qc analyze --dir <path>` and get a per-file report of head silence, title speech, title-body gap, and tail silence"
    - "Files with anomalous structure are flagged with clear threshold violations"
    - "User can export the report as JSON with `--json <path>`"
  artifacts:
    - path: "host/Ams.Core/Audio/QualityControl/AudioQcModels.cs"
      provides: "Record types for QC analysis results and thresholds"
    - path: "host/Ams.Core/Audio/QualityControl/AudioQcAnalyzer.cs"
      provides: "FFmpeg silencedetect invocation + head/tail structure analysis"
      exports: ["AudioQcAnalyzer"]
    - path: "host/Ams.Cli/Commands/QcCommand.cs"
      provides: "CLI verb: qc analyze --dir <path>"
      exports: ["QcCommand"]
    - path: "host/Ams.Tests/Audio/QualityControl/AudioQcAnalyzerTests.cs"
      provides: "Unit tests for silence parsing and structure analysis logic"
  key_links:
    - from: "host/Ams.Cli/Commands/QcCommand.cs"
      to: "host/Ams.Core/Audio/QualityControl/AudioQcAnalyzer.cs"
      via: "Direct instantiation (no DI needed -- stateless analyzer)"
      pattern: "new AudioQcAnalyzer"
    - from: "host/Ams.Cli/Program.cs"
      to: "host/Ams.Cli/Commands/QcCommand.cs"
      via: "rootCommand.AddCommand(QcCommand.Create())"
      pattern: "QcCommand\\.Create"
---

<objective>
Add a standalone `qc analyze` CLI command that analyzes audiobook chapter files using ffmpeg silencedetect to map head/tail silence structure and flag anomalies.

Purpose: Replace external QC tools that produce false positives by using proper threshold-aware silence detection that understands the standard audiobook chapter structure (leading silence -> title speech -> title-body gap -> body -> trailing silence).

Output: New CLI command `qc analyze --dir <path>` that produces a console table report and optional JSON export.
</objective>

<execution_context>
@/home/cari/.claude/get-shit-done/workflows/execute-plan.md
@/home/cari/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@host/Ams.Core/Pipeline/SentenceRefinementService.cs (lines 177-223 — existing ffmpeg silencedetect process invocation + ParseSilenceOutput pattern to reuse)
@host/Ams.Core/Audio/AudioDefaults.cs (shared silence constants)
@host/Ams.Cli/Commands/TreatCommand.cs (CLI command pattern — static Create() returning Command)
@host/Ams.Cli/Program.cs (command registration site)
@host/Ams.Cli/Services/PlugalyzerService.cs (ProcessStartInfo pattern for external process invocation)

<interfaces>
<!-- Existing silence detection pattern from SentenceRefinementService (lines 177-223) -->
From host/Ams.Core/Pipeline/SentenceRefinementService.cs:
```csharp
// This is the pattern to follow for ffmpeg process invocation:
public sealed record SilenceInfo(double Start, double End, double Duration, double Confidence);

private async Task<SilenceInfo[]> DetectSilencesAsync(string audioPath, double thresholdDb, double minDurationSec)
{
    var ffmpegExe = Environment.GetEnvironmentVariable("FFMPEG_EXE");
    if (string.IsNullOrWhiteSpace(ffmpegExe)) ffmpegExe = "ffmpeg";
    var psi = new ProcessStartInfo
    {
        FileName = ffmpegExe!,
        Arguments = $"-i \"{audioPath}\" -af silencedetect=noise={thresholdDb}dB:duration={minDurationSec} -f null -",
        RedirectStandardError = true,
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    using var p = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start ffmpeg");
    await p.WaitForExitAsync(cts.Token);
    var stderr = await p.StandardError.ReadToEndAsync();
    return ParseSilenceOutput(stderr);
}
```

From host/Ams.Core/Audio/AudioDefaults.cs:
```csharp
public static class AudioDefaults
{
    public const double SilenceThresholdDb = -55.0;
    public static readonly TimeSpan MinimumSilenceDuration = TimeSpan.FromMilliseconds(200);
}
```

<!-- CLI command pattern -->
From host/Ams.Cli/Program.cs (registration):
```csharp
rootCommand.AddCommand(QcCommand.Create()); // Add alongside existing commands
```

<!-- Logging facade -->
```csharp
using Ams.Core.Common; // provides static Log class
Log.Info("message {Param}", value);
Log.Debug("...");
Log.Error("...");
Log.Warn("...");
```
</interfaces>
</context>

<tasks>

<task type="auto" tdd="true">
  <name>Task 1: Core QC analyzer service with models and tests</name>
  <files>
    host/Ams.Core/Audio/QualityControl/AudioQcModels.cs,
    host/Ams.Core/Audio/QualityControl/AudioQcAnalyzer.cs,
    host/Ams.Tests/Audio/QualityControl/AudioQcAnalyzerTests.cs
  </files>
  <behavior>
    - ParseSilenceRegions: given ffmpeg silencedetect stderr text, returns ordered list of SilenceRegion(Start, End, Duration)
    - ParseSilenceRegions: handles empty output (no silence detected) returning empty list
    - ParseSilenceRegions: handles trailing silence_start with no matching silence_end (open-ended silence extending to file end — use duration from ffprobe/metadata)
    - AnalyzeStructure: given silence regions + total file duration, identifies head silence (first region starting at or near 0.0), title-body gap (second silence region), and tail silence (last region ending at or near total duration)
    - AnalyzeStructure: computes title duration as the speech between end of head silence and start of title-body gap
    - AnalyzeStructure: returns null for title fields when there is no second silence region (single-silence files)
    - FlagAnomalies: given thresholds and analysis result, flags head silence < 0.5s or > 1.0s, tail silence < 2.0s or > 5.0s, title-body gap < 1.0s or > 2.5s
  </behavior>
  <action>
    Create `AudioQcModels.cs` in `host/Ams.Core/Audio/QualityControl/` with these records:

    ```csharp
    // Represents a single silence region detected by ffmpeg
    public sealed record SilenceRegion(double Start, double End, double Duration);

    // Thresholds for flagging anomalies (all in seconds)
    public sealed record QcThresholds
    {
        public double MinHeadSilence { get; init; } = 0.5;
        public double MaxHeadSilence { get; init; } = 1.0;
        public double MinTailSilence { get; init; } = 2.0;
        public double MaxTailSilence { get; init; } = 5.0;
        public double MinTitleBodyGap { get; init; } = 1.0;
        public double MaxTitleBodyGap { get; init; } = 2.5;
    }

    // Per-file analysis result
    public sealed record ChapterQcResult
    {
        public required string FileName { get; init; }
        public required double DurationSec { get; init; }
        public int Channels { get; init; }
        public int SampleRate { get; init; }
        public double HeadSilenceSec { get; init; }
        public double? TitleDurationSec { get; init; }     // null if no title detected
        public double? TitleBodyGapSec { get; init; }      // null if no gap detected
        public double TailSilenceSec { get; init; }
        public IReadOnlyList<SilenceRegion> AllSilences { get; init; } = [];
        public IReadOnlyList<string> Flags { get; init; } = [];
    }
    ```

    Create `AudioQcAnalyzer.cs` in same directory with:

    1. **`ParseSilenceRegions(string ffmpegStderr)`** — public static, pure function. Reuse the regex pattern from `SentenceRefinementService.ParseSilenceOutput` but return `SilenceRegion[]`. Parse `silence_start:` and `silence_end:` / `silence_duration:` pairs. Use `CultureInfo.InvariantCulture` for double parsing (not the bare `double.Parse` from the existing code — that is a latent locale bug).

    2. **`AnalyzeStructure(IReadOnlyList<SilenceRegion> silences, double totalDurationSec)`** — public static, pure function. Logic:
       - Head silence: first region where `Start < 0.05` (near start). HeadSilenceSec = that region's Duration. If none, HeadSilenceSec = 0.
       - Tail silence: last region where `End > totalDurationSec - 0.05` (near end). TailSilenceSec = that region's Duration. If none, TailSilenceSec = 0.
       - Title-body gap: if there are at least 2 silence regions after head, the second region is the title-body gap. TitleBodyGapSec = its Duration.
       - Title duration: speech between end of head silence and start of title-body gap.
       - Returns a partial `ChapterQcResult` (FileName/Channels/SampleRate filled by caller).

    3. **`FlagAnomalies(ChapterQcResult result, QcThresholds thresholds)`** — public static, pure function. Returns list of string flags like "HEAD_SILENCE_SHORT (0.32s < 0.50s min)", "TAIL_SILENCE_LONG (6.10s > 5.00s max)".

    4. **`AnalyzeFileAsync(string filePath, double noiseDb, double minSilenceDurationSec, QcThresholds thresholds, CancellationToken ct)`** — runs ffmpeg silencedetect + ffprobe for metadata, composes the full `ChapterQcResult`. Use `FFMPEG_EXE` env var with fallback to `"ffmpeg"` (same as existing pattern). Run `ffprobe -v quiet -print_format json -show_format -show_streams <file>` to get duration, channels, sample_rate.

    Write tests in `AudioQcAnalyzerTests.cs` covering the pure functions (ParseSilenceRegions, AnalyzeStructure, FlagAnomalies) with representative ffmpeg output strings. Do NOT test AnalyzeFileAsync (requires ffmpeg binary).

    Use `CultureInfo.InvariantCulture` everywhere doubles are parsed — the existing `SentenceRefinementService` code has a latent locale bug with bare `double.Parse`.
  </action>
  <verify>
    <automated>dotnet test host/Ams.Tests --filter "FullyQualifiedName~AudioQcAnalyzer" --no-restore -v minimal</automated>
  </verify>
  <done>
    - AudioQcModels.cs defines SilenceRegion, QcThresholds, ChapterQcResult records
    - AudioQcAnalyzer.cs has ParseSilenceRegions, AnalyzeStructure, FlagAnomalies (pure functions) and AnalyzeFileAsync (ffmpeg process)
    - All pure function tests pass
    - InvariantCulture used for all double parsing
  </done>
</task>

<task type="auto">
  <name>Task 2: QC CLI command with console table and JSON export</name>
  <files>
    host/Ams.Cli/Commands/QcCommand.cs,
    host/Ams.Cli/Program.cs
  </files>
  <action>
    Create `QcCommand.cs` as a static class following the same pattern as `TreatCommand.Create()` / `AsrCommand.Create()`:

    ```
    qc analyze --dir <path> [--noise -40] [--min-silence 0.05] [--json <output.json>]
                [--min-head-silence 0.5] [--max-head-silence 1.0]
                [--min-tail-silence 2.0] [--max-tail-silence 5.0]
                [--min-title-gap 1.0] [--max-title-gap 2.5]
    ```

    Structure:
    - Top-level `qc` command
    - Subcommand `analyze`
    - `--dir` (required): Directory containing audio files (*.mp3, *.wav, *.flac, *.m4a)
    - `--noise` (default: -40): Silence detection noise floor in dB (use -40 per user's analysis, NOT AudioDefaults which is -55 and tuned for ASR chunking)
    - `--min-silence` (default: 0.05): Minimum silence duration in seconds for detection
    - `--json`: Optional path to write JSON report
    - Threshold overrides with sensible defaults from QcThresholds

    Handler logic:
    1. Enumerate audio files in `--dir` matching *.mp3, *.wav, *.flac, *.m4a (sorted alphabetically)
    2. If no files found, print error and exit 1
    3. For each file, call `AudioQcAnalyzer.AnalyzeFileAsync(...)` with a progress counter ("Analyzing 3/24: Chapter03.mp3...")
    4. Print a Spectre.Console table with columns: File | Duration | Head | Title | Gap | Tail | Flags
       - Duration formatted as mm:ss.f
       - Head/Title/Gap/Tail formatted as X.XXs
       - Flags column: comma-separated flag names, colored red with Spectre markup
       - Null title/gap shown as "-"
    5. After the table, print a summary line: "N files analyzed, M flagged"
    6. If `--json` specified, serialize List<ChapterQcResult> to JSON with System.Text.Json (indented) and write to path

    Register in Program.cs:
    ```csharp
    rootCommand.AddCommand(QcCommand.Create());
    ```
    Add it after the existing `TreatCommand.Create()` line.

    Error handling: wrap each file analysis in try/catch, log the error with `Log.Error`, mark the file as having a "ANALYSIS_FAILED" flag, and continue to next file. Do NOT abort the batch for a single file failure.

    Note: This command is standalone — no workspace, no book index, no REPL context. It takes a directory and analyzes the files directly.
  </action>
  <verify>
    <automated>dotnet build host/Ams.Cli -c Debug --no-restore -v minimal && dotnet run --project host/Ams.Cli -- qc analyze --help</automated>
  </verify>
  <done>
    - `qc analyze --help` prints usage with all options
    - `qc analyze --dir <path>` produces a formatted console table with per-file metrics
    - `--json <path>` writes structured JSON report
    - Anomalous files are flagged based on threshold comparison
    - Command registered in Program.cs and accessible from CLI
    - Single file failures logged and skipped without aborting batch
  </done>
</task>

</tasks>

<verification>
1. `dotnet build host/Ams.Cli -c Debug --no-restore` compiles without errors
2. `dotnet test host/Ams.Tests --filter "FullyQualifiedName~AudioQcAnalyzer" --no-restore` all tests pass
3. `dotnet run --project host/Ams.Cli -- qc analyze --help` shows usage
4. Manual: `dotnet run --project host/Ams.Cli -- qc analyze --dir <path-to-chapter-mp3s>` produces table output with head/tail/gap analysis
</verification>

<success_criteria>
- QC analyzer correctly maps silence regions from ffmpeg silencedetect output
- Head structure (silence -> title -> gap -> body) and tail silence are identified per file
- Anomalies flagged against configurable thresholds
- Console table and JSON export both work
- Pure analysis functions are unit tested
- No dependency on AMS pipeline (book index, ASR, workspace) -- fully standalone
</success_criteria>

<output>
After completion, create `.planning/quick/8-add-audiobook-qc-cli-command-ffmpeg-base/8-SUMMARY.md`
</output>
