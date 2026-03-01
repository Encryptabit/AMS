---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs"
access_modifier: "private"
complexity: 4
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/error-handling
  - llm/validation
---
# ResampleScratch::Release
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs`

## Summary
**Releases native resampling scratch buffers and returns the object to an uninitialized state.**

This private cleanup routine frees all unmanaged channel sample buffers owned by `ResampleScratch` and resets allocator state. It no-ops when `_buffers` is null; otherwise it iterates `_channels`, releases each non-null plane with `ffmpeg.av_free`, then frees the pointer array itself and nulls it out. Finally it resets `_channels`, `_capacity`, and `_format` (`AV_SAMPLE_FMT_NONE`) so subsequent `Rent` calls reinitialize safely.


#### [[ResampleScratch.Release]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void Release()
```

**Called-by <-**
- [[ResampleScratch.Dispose]]
- [[ResampleScratch.Rent]]

