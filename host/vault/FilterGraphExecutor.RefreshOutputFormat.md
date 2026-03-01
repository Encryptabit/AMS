---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# FilterGraphExecutor::RefreshOutputFormat
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Updates the executor’s output sample-rate field from the sink’s negotiated format when available.**

`RefreshOutputFormat` synchronizes executor sample-rate state with the configured sink after graph setup. It no-ops when `_sink` is null, otherwise reads `ffmpeg.av_buffersink_get_sample_rate(_sink)` and updates `_sampleRate` only when the reported rate is positive. This preserves the constructor-initialized rate unless sink negotiation provides a valid override.


#### [[FilterGraphExecutor.RefreshOutputFormat]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void RefreshOutputFormat()
```

**Called-by <-**
- [[FilterGraphExecutor..ctor]]

