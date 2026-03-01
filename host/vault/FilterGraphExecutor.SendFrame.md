---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FilterGraphExecutor::SendFrame
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Populates and pushes one audio frame chunk into the FFmpeg filter source, signaling whether more input is still accepted.**

`SendFrame` writes a chunk of planar input samples into a reusable FFmpeg frame and submits it to the source filter node. It validates `state.Frame` is initialized, makes the frame writable (`av_frame_make_writable`), sets `nb_samples`/`pts`, then copies interleaved-per-sample/channel data from `state.Buffer.Planar[ch][offset + i]` into `frame->data[0]`. Submission uses `av_buffersrc_add_frame_flags(..., BufferSrcFlagKeepRef)` and treats `AVERROR_EOF` as a non-exceptional early-stop signal (`false`), while all other negative results flow through `FfUtils.ThrowIfError` before returning `true`.


#### [[FilterGraphExecutor.SendFrame]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraphRunner.FilterGraphExecutor.SendFrame(Ams.Core.Services.Integrations.FFmpeg.FfFilterGraphRunner.GraphInputState,System.Int32,System.Int32,System.Int64)">
    <summary>
    Sends a frame to the filter graph.
    Returns false if the filter signals EOF (doesn't need more input), true otherwise.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private bool SendFrame(FfFilterGraphRunner.GraphInputState state, int offset, int sampleCount, long pts)
```

**Calls ->**
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FilterGraphExecutor.SendAllFrames]]

