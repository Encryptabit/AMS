---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AsrProcessor::ShouldRetryWithoutDtw
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Determines whether a Whisper result should be rerun without DTW based on effective DTW settings and transcript-to-audio coverage metrics.**

`ShouldRetryWithoutDtw` computes `audioDurationSec` and `transcriptEndSec` via helper methods, derives `coverage = transcriptEndSec / audioDurationSec` (or `0` when duration is non-positive), and returns a retry decision predicate. The method gates retries on DTW being effectively enabled (`IsDtwEffectivelyEnabled(options)`) and then evaluates transcript coverage against internal thresholds/conditions to detect likely truncation. Its `out` values expose diagnostics used by the caller for structured warning logs.


#### [[AsrProcessor.ShouldRetryWithoutDtw]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool ShouldRetryWithoutDtw(AsrOptions options, AudioBuffer buffer, AsrResponse response, out double audioDurationSec, out double transcriptEndSec, out double coverage)
```

**Calls ->**
- [[AsrProcessor.ComputeAudioDurationSeconds]]
- [[AsrProcessor.ComputeTranscriptEndSeconds]]
- [[AsrProcessor.IsDtwEffectivelyEnabled]]

**Called-by <-**
- [[AsrProcessor.TranscribeWithWhisperNetAsync]]

