---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Audio/AudioSpliceService.cs"
access_modifier: "private"
complexity: 4
fan_in: 3
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioSpliceService::Crossfade
**Path**: `Projects/AMS/host/Ams.Core/Audio/AudioSpliceService.cs`

## Summary
**Join two audio buffers with an FFmpeg crossfade, with safe concat fallback for trivial or empty cases.**

`Crossfade` conditionally blends two buffers with FFmpeg `acrossfade`. It first falls back to `AudioBuffer.Concat(a, b)` when fade duration is negligible (`<= 0.001`) or either input is empty, avoiding filter overhead and invalid transitions. Otherwise it builds two graph inputs, composes a culture-invariant filter spec that normalizes both streams to float sample format and applies `acrossfade` with `d={durationSec:F6}` and symmetric curve parameters (`c1`, `c2`), then executes via `FfFilterGraphRunner.Apply`. The returned `AudioBuffer` is the rendered crossfaded result.


#### [[AudioSpliceService.Crossfade]]
##### What it does:
<member name="M:Ams.Core.Audio.AudioSpliceService.Crossfade(Ams.Core.Artifacts.AudioBuffer,Ams.Core.Artifacts.AudioBuffer,System.Double,System.String)">
    <summary>
    Crossfades two audio buffers using FFmpeg's acrossfade filter.
    Falls back to simple concatenation when crossfade is negligible or a buffer is empty.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static AudioBuffer Crossfade(AudioBuffer a, AudioBuffer b, double durationSec, string curve)
```

**Calls ->**
- [[AudioBuffer.Concat]]
- [[FfFilterGraphRunner.Apply_2]]

**Called-by <-**
- [[AudioSpliceService.DeleteRegion]]
- [[AudioSpliceService.InsertAtPoint]]
- [[AudioSpliceService.ReplaceSegment]]

