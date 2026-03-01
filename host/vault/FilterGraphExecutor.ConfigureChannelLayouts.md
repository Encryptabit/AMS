---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FilterGraphExecutor::ConfigureChannelLayouts
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Sets the output sink’s channel layout constraint to a metadata/default-derived layout and fails fast when FFmpeg rejects it.**

`ConfigureChannelLayouts` derives the sink’s allowed layout string from `_primaryMetadata?.CurrentChannelLayout` with fallback to `AudioBufferMetadata.DescribeDefaultLayout(_channels)`, and then to `"mono"` if still blank. It applies this value to `_sink` through `ffmpeg.av_opt_set(..., "ch_layouts", layoutName, AV_OPT_SEARCH_CHILDREN)`. If the option set fails (`layoutResult < 0`), it formats the FFmpeg error via `FfUtils.FormatError` and throws an `InvalidOperationException`.


#### [[FilterGraphExecutor.ConfigureChannelLayouts]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void ConfigureChannelLayouts()
```

**Calls ->**
- [[AudioBufferMetadata.DescribeDefaultLayout]]
- [[FfUtils.FormatError]]

**Called-by <-**
- [[FilterGraphExecutor.ConfigureSinkFormat]]

