---
namespace: "Ams.Core.Services"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/AsrService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/validation
---
# AsrService::TranscribeAsync
**Path**: `Projects/AMS/host/Ams.Core/Services/AsrService.cs`

## Summary
**Starts transcription for a chapter by obtaining the prepared audio buffer and delegating to the shared ASR processor.**

This public async-facing method is a thin orchestration wrapper for chapter transcription. It validates `chapter` via `ArgumentNullException.ThrowIfNull`, resolves a preprocessed ASR-ready `AudioBuffer` using `ResolveAsrReadyBuffer(chapter)`, and directly returns `AsrProcessor.TranscribeBufferAsync(buffer, options, cancellationToken)`.


#### [[AsrService.TranscribeAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<AsrResponse> TranscribeAsync(ChapterContext chapter, AsrOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[AsrProcessor.TranscribeBufferAsync]]
- [[AsrService.ResolveAsrReadyBuffer]]

