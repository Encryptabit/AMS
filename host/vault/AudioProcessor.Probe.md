---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AudioProcessor.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/validation
  - llm/utility
---
# AudioProcessor::Probe
**Path**: `Projects/AMS/host/Ams.Core/Processors/AudioProcessor.cs`

## Summary
**Validates an audio file path and retrieves container/stream metadata via the FFmpeg-backed decoder probe.**

`Probe` is a thin synchronous wrapper that enforces a non-empty input path using `ArgumentException.ThrowIfNullOrWhiteSpace(path)` and then delegates directly to `FfDecoder.Probe(path)`. It returns the resulting `AudioInfo` metadata without additional transformation or fallback behavior.


#### [[AudioProcessor.Probe]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioInfo Probe(string path)
```

**Calls ->**
- [[FfDecoder.Probe]]

**Called-by <-**
- [[AudioController.GetChapterRegionAudio]]

