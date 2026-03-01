---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# FfUtils::CheckSampleFormat
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfUtils.cs`

## Summary
**Validates codec/sample-format arguments and currently returns a placeholder success code.**

`CheckSampleFormat` currently acts as an input guard/stub rather than a true capability probe. It throws `ArgumentNullException` when `codec` is null and `ArgumentException` when `format` is `AV_SAMPLE_FMT_NONE`, then returns `0` unconditionally for all other inputs. A TODO comment indicates planned migration to `avcodec_get_supported_config` for real runtime negotiation.


#### [[FfUtils.CheckSampleFormat]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static int CheckSampleFormat(AVCodec* codec, AVSampleFormat format)
```

