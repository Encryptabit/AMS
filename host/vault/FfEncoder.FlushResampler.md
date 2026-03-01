---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "private"
complexity: 5
fan_in: 2
fan_out: 4
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfEncoder::FlushResampler
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`

## Summary
**Flushes remaining resampler output into the encoder so trailing audio is not lost.**

This method drains delayed samples buffered inside the FFmpeg resampler after normal input is exhausted. It repeatedly computes a destination capacity (`ComputeResampleOutputSamples(..., inSamples: 0)` with fallback to `cc->frame_size`), prepares a writable frame (`EnsureFrameCapacity`, `av_frame_make_writable`), then calls `swr_convert(..., null, 0)` to pull pending output. Each produced chunk is timestamped with `pts`, sent to the encoder (`avcodec_send_frame`), and packet-drained via `DrainEncoder`; the loop exits when no more samples are produced.


#### [[FfEncoder.FlushResampler]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void FlushResampler(int sourceSampleRate, int targetSampleRate, SwrContext* resampler, AVFrame* frame, AVCodecContext* cc, AVStream* stream, AVFormatContext* fmt, ref long pts)
```

**Calls ->**
- [[FfEncoder.DrainEncoder]]
- [[FfEncoder.EnsureFrameCapacity]]
- [[FfUtils.ComputeResampleOutputSamples]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FfEncoder.EncodeBuffer]]
- [[StreamingEncoderSink.Complete]]

