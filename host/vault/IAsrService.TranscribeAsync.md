---
namespace: "Ams.Core.Services.Interfaces"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Interfaces/IAsrService.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/async
  - llm/di
  - llm/validation
  - llm/error-handling
---
# IAsrService::TranscribeAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/Interfaces/IAsrService.cs`

## Summary
**Transcribe chapter audio into an `AsrResponse` by preparing the chapter buffer and delegating execution to the shared ASR processor.**

In `AsrService`, `TranscribeAsync` is a thin async orchestration wrapper: it validates `chapter` via `ArgumentNullException.ThrowIfNull`, resolves an ASR-ready `AudioBuffer`, and directly returns `AsrProcessor.TranscribeBufferAsync(buffer, options, cancellationToken)` (no extra await/state machine). The buffer path goes through `ResolveAsrReadyBuffer`, which selects the first `chapter.Descriptor.AudioBuffers` entry, loads it with `chapter.Audio.Load`, and normalizes it using `AsrAudioPreparer.PrepareForAsr`. Invalid chapter audio state is surfaced by exceptions from the resolver path (for example missing buffers / unloadable buffer), and `RunWhisperAsync` is the call site that passes constructed `AsrOptions` into this method.


#### [[IAsrService.TranscribeAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task<AsrResponse> TranscribeAsync(ChapterContext chapter, AsrOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Called-by <-**
- [[GenerateTranscriptCommand.RunWhisperAsync]]

