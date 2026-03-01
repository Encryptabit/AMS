---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# FilterGraphExecutor::ConfigureSinkFormat
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Configures the buffer sink to emit float-sample audio with the executor’s expected channel layout constraints.**

`ConfigureSinkFormat` applies output format constraints to the FFmpeg sink context in a fixed two-step sequence. It sets the sink sample format by calling `ConfigureIntOption("sample_fmts", (int)AVSampleFormat.AV_SAMPLE_FMT_FLT)`, then configures accepted channel layouts via `ConfigureChannelLayouts()`. The method is a thin orchestration layer with no branching or local validation.


#### [[FilterGraphExecutor.ConfigureSinkFormat]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void ConfigureSinkFormat()
```

**Calls ->**
- [[FilterGraphExecutor.ConfigureChannelLayouts]]
- [[FilterGraphExecutor.ConfigureIntOption]]

**Called-by <-**
- [[FilterGraphExecutor.SetupSink]]

