---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptIndexService::ResolveDefaultAudioPath
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptIndexService.cs`

## Summary
**Determines the transcript’s default audio path from chapter descriptors, falling back to a root-path chapter WAV convention.**

This private static resolver chooses a default audio file path from chapter metadata with a simple two-step fallback. It first inspects `context.Descriptor.AudioBuffers.FirstOrDefault()` and returns `descriptor.Path` when present and non-whitespace. If no usable buffer path exists, it synthesizes a conventional filename using `Path.Combine(context.Descriptor.RootPath, $"{context.Descriptor.ChapterId}.wav")`.


#### [[TranscriptIndexService.ResolveDefaultAudioPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolveDefaultAudioPath(ChapterContext context)
```

**Called-by <-**
- [[TranscriptIndexService.BuildTranscriptIndexAsync]]

