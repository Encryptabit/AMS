---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
complexity: 7
fan_in: 2
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/di
  - llm/error-handling
---
# FilterGraphExecutor::Drain
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Drains all currently available output frames from the filter graph and forwards them to the configured sink or accumulator.**

`Drain` repeatedly pulls available frames from the filter sink using `av_buffersink_get_frame(_sink, _outputFrame)` until FFmpeg reports `EAGAIN` or `EOF`. For each successful frame, it validates return codes via `FfUtils.ThrowIfError`, then routes output either to `_frameSink.Consume(_outputFrame)` (streaming mode) or `_accumulator.Add(_outputFrame)` (buffer-return mode). Each iteration releases frame references with `av_frame_unref(_outputFrame)`, and when `final` is `true` it performs an extra final `av_frame_unref` after the loop.


#### [[FilterGraphExecutor.Drain]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void Drain(bool final = false)
```

**Calls ->**
- [[AudioAccumulator.Add]]
- [[IAudioFrameSink.Consume]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FilterGraphExecutor.Process]]
- [[FilterGraphExecutor.SendAllFrames]]

