---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "private"
complexity: 4
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfEncoder::ResolveEncoding
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`

## Summary
**Resolves encoding configuration (codec + sample format) from a target output bit depth.**

This mapper translates requested PCM bit depth into the FFmpeg encoder codec ID and codec-context sample format used by the pipeline. It uses a switch expression with explicit mappings (`16 -> PCM_S16LE/S16`, `24 -> PCM_S24LE/S32`, `32 -> PCM_F32LE/FLT`) and rejects unsupported depths by throwing `NotSupportedException`. The tuple output is consumed by encoder initialization to configure format-compatible output.


#### [[FfEncoder.ResolveEncoding]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (AVCodecID CodecId, AVSampleFormat SampleFormat) ResolveEncoding(int bitDepth)
```

**Called-by <-**
- [[FfEncoder.Encode]]
- [[StreamingEncoderSink.Initialize]]

