---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FilterGraphExecutor::ConfigureIntOption
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Applies a numeric option to the FFmpeg sink context using binary option payload semantics and fails fast on configuration errors.**

`ConfigureIntOption` sets an integer-valued sink option by building a two-element unmanaged buffer (`stackalloc int[2]`) containing `{ value, -1 }`, where `-1` acts as the FFmpeg list terminator. It passes that buffer to `ffmpeg.av_opt_set_bin(_sink, optionName, (byte*)buffer, sizeof(int) * 2, ffmpeg.AV_OPT_SEARCH_CHILDREN)` and throws `InvalidOperationException` with `FfUtils.FormatError(result)` when the call fails (`result < 0`).


#### [[FilterGraphExecutor.ConfigureIntOption]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void ConfigureIntOption(string optionName, int value)
```

**Calls ->**
- [[FfUtils.FormatError]]

**Called-by <-**
- [[FilterGraphExecutor.ConfigureSinkFormat]]

