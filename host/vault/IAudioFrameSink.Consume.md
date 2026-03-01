---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/di
  - llm/utility
---
# IAudioFrameSink::Consume
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Defines the sink-side contract for consuming each output audio frame emitted by the FFmpeg filter runner.**

`Consume` on `FfFilterGraphRunner.IAudioFrameSink` is an interface callback declaration (no implementation) for handling each produced `AVFrame*` from the filter graph. In `FilterGraphExecutor.Drain`, it is invoked on every successful `av_buffersink_get_frame` when a sink is present (`_frameSink.Consume(_outputFrame)`), and the frame is immediately `av_frame_unref`’d afterward. This makes the pointer a transient, call-scoped input for sink implementations.


#### [[IAudioFrameSink.Consume]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
void Consume(AVFrame* frame)
```

**Called-by <-**
- [[FilterGraphExecutor.Drain]]

