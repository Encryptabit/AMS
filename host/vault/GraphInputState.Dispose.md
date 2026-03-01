---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# GraphInputState::Dispose
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Cleans up native frame and channel-layout resources associated with a graph input state.**

`GraphInputState.Dispose` releases the unmanaged resources owned by a single input state. It conditionally frees `Frame` with `ffmpeg.av_frame_free` and nulls the pointer to prevent reuse, then always uninitializes the embedded `AVChannelLayout` by pinning `Layout` and calling `ffmpeg.av_channel_layout_uninit`. The method performs no exception translation and assumes FFmpeg cleanup calls are safe on current state.


#### [[GraphInputState.Dispose]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Dispose()
```

**Called-by <-**
- [[FilterGraphExecutor.Dispose]]

