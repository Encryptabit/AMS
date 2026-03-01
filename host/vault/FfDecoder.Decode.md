---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs"
access_modifier: "public"
complexity: 56
fan_in: 1
fan_out: 11
tags:
  - method
  - danger/high-complexity
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfDecoder::Decode
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs`

> [!danger] High Complexity (56)
> Cyclomatic complexity: 56. Consider refactoring into smaller methods.

## Summary
**Decodes an audio file into a normalized `AudioBuffer` (optionally resampled/sliced) with preserved provenance metadata.**

This method performs full FFmpeg-based decode into an in-memory planar `AudioBuffer` with optional channel/sample-rate conversion and partial time-range extraction. It validates file existence, opens media/stream/codec contexts with `ThrowIfError`, selects the best audio stream, configures an `SwrContext` when format/rate/channel differ from requested targets, then decodes packets/frames while appending (or resampling) float samples per channel. For ranged decodes, it seeks to start time, caps decode work, and post-trims sample lists to exact `[Start, Duration]` boundaries before materializing the buffer. It attaches rich source/current metadata (container, codec, timings, layouts, tags) and aggressively frees FFmpeg resources in nested `finally` blocks (`swr_free`, `av_channel_layout_uninit`, `avcodec_free_context`, `avformat_close_input`).


#### [[FfDecoder.Decode]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioBuffer Decode(string path, AudioDecodeOptions options)
```

**Calls ->**
- [[AudioBuffer.UpdateMetadata]]
- [[AudioBufferMetadata.DescribeDefaultLayout]]
- [[FfDecoder.AppendSamples]]
- [[FfPacket.Unref]]
- [[FfDecoder.GetSampleFormatName]]
- [[FfDecoder.PtrToStringUtf8]]
- [[FfDecoder.ReadTags]]
- [[FfSession.EnsureInitialized]]
- [[FfUtils.CloneOrDefault]]
- [[FfUtils.CreateDefaultChannelLayout]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[AudioProcessor.Decode]]

