---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs"
access_modifier: "public"
complexity: 6
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfUtils::SelectSampleRate
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs`

## Summary
**Chooses a usable sample rate for a codec context by validating the requested rate against FFmpeg-reported capabilities.**

`SelectSampleRate` queries codec-supported sample rates via `avcodec_get_supported_config(..., AV_CODEC_CONFIG_SAMPLE_RATE, ...)` and returns the error code immediately if probing fails (`ret < 0`). If the codec does not publish a list, it defaults to `44100`. Otherwise it scans reported rates and returns the one matching `ctx->sample_rate`; if no match is found, it emits an FFmpeg error log and returns `0` to indicate unsupported selection.


#### [[FfUtils.SelectSampleRate]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static int SelectSampleRate(AVCodecContext* ctx)
```

