---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AudioProcessor.cs"
access_modifier: "public"
complexity: 4
fan_in: 3
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/validation
  - llm/utility
  - llm/error-handling
---
# AudioProcessor::EncodeWav
**Path**: `Projects/AMS/host/Ams.Core/Processors/AudioProcessor.cs`

## Summary
**Encodes an in-memory audio buffer to a WAV file on disk using FFmpeg-backed encoding settings.**

`EncodeWav` writes an `AudioBuffer` to a WAV file by null-checking `buffer`, resolving default encode options when omitted, and ensuring the destination directory exists via `Directory.CreateDirectory` on the full path’s parent. It creates the output file stream with `File.Create`, passes buffer/stream/options to `FfEncoder.EncodeToCustomStream`, and explicitly flushes the stream afterward. The method performs synchronous file I/O with no retry or fallback path.


#### [[AudioProcessor.EncodeWav]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void EncodeWav(string path, AudioBuffer buffer, AudioEncodeOptions options = null)
```

**Calls ->**
- [[FfEncoder.EncodeToCustomStream]]

**Called-by <-**
- [[AudioTreatmentService.TreatChapterCoreAsync]]
- [[PolishService.PersistCorrectedBuffer]]
- [[UndoService.SaveOriginalSegment]]

