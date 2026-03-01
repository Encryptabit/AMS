---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "public"
complexity: 2
fan_in: 4
fan_out: 2
tags:
  - method
  - llm/async
  - llm/validation
  - llm/utility
---
# AsrProcessor::TranscribeBufferAsync
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Runs asynchronous transcription on an existing audio buffer after validating ASR model configuration.**

This `TranscribeBufferAsync` overload accepts a prebuilt `AudioBuffer`, validates `options` (null-check + `EnsureModelPath`), and performs an early cancellation check. It does not decode or reshape audio; instead it forwards the provided buffer directly to `TranscribeBufferInternalAsync` for the shared Whisper transcription flow. The method serves as a thin async entry point for callers that already have decoded audio.


#### [[AsrProcessor.TranscribeBufferAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Task<AsrResponse> TranscribeBufferAsync(AudioBuffer buffer, AsrOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[AsrProcessor.EnsureModelPath]]
- [[AsrProcessor.TranscribeBufferInternalAsync]]

**Called-by <-**
- [[AsrService.TranscribeAsync]]
- [[PickupMatchingService.MatchPickupCrxAsync]]
- [[PickupMatchingService.MatchSinglePickupAsync]]
- [[PolishVerificationService.RevalidateSegmentAsync]]

