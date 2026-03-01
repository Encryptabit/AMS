---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "public"
complexity: 6
fan_in: 0
fan_out: 4
tags:
  - method
  - llm/entry-point
  - llm/data-access
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# StreamingEncoderSink::Consume
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`

## Summary
**Consumes an input audio frame, resamples it to target format, and emits encoded packets to the configured output stream.**

This method ingests a source frame into the active streaming encoder pipeline, enforcing initialization and ignoring null/zero-sample input. It computes required output capacity (`ComputeResampleOutputSamples`), prepares a writable reusable frame (`EnsureFrameCapacity`, `av_frame_make_writable`), converts samples with `swr_convert`, and if conversion yields data, stamps `pts` and advances `_pts`. It then submits the frame (`avcodec_send_frame`) and drains available encoded packets via `DrainEncoder`.


#### [[StreamingEncoderSink.Consume]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Consume(AVFrame* frame)
```

**Calls ->**
- [[FfEncoder.DrainEncoder]]
- [[FfEncoder.EnsureFrameCapacity]]
- [[FfUtils.ComputeResampleOutputSamples]]
- [[FfUtils.ThrowIfError]]

