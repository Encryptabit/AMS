---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 5
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfEncoder::EncodeBuffer
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`

## Summary
**Feeds resampled chunked audio data into the FFmpeg encoder and drains encoded packets to the output container.**

This method chunk-encodes an `AudioBuffer` by iterating source samples in `DefaultChunkSamples` blocks, resampling each block into an FFmpeg frame, and pushing frames to the encoder. For each chunk it computes destination capacity (`ComputeResampleOutputSamples`), ensures/writes frame buffers (`EnsureFrameCapacity`, `av_frame_make_writable`), maps pinned planar channel pointers with cursor offset, converts via `swr_convert`, then sends frames and drains packets (`DrainEncoder`). It tracks monotonically increasing `pts` in output-sample units and, after source exhaustion, calls `FlushResampler` to emit delayed resampler tail samples.


#### [[FfEncoder.EncodeBuffer]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void EncodeBuffer(AudioBuffer buffer, AVCodecContext* cc, AVStream* stream, AVFormatContext* fmt, SwrContext* resampler, AVFrame* frame, nint[] channelPointers, int targetSampleRate)
```

**Calls ->**
- [[FfEncoder.DrainEncoder]]
- [[FfEncoder.EnsureFrameCapacity]]
- [[FfEncoder.FlushResampler]]
- [[FfUtils.ComputeResampleOutputSamples]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FfEncoder.Encode]]

