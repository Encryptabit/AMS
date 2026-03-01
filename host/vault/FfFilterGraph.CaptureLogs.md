---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 1
fan_in: 3
fan_out: 4
tags:
  - method
  - llm/entry-point
  - llm/utility
  - llm/error-handling
---
# FfFilterGraph::CaptureLogs
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Execute the current graph in discard-output mode while collecting and returning FFmpeg logs.**

`CaptureLogs()` builds the current filter spec once (`BuildSpec()`), then runs execution inside `FfLogCapture.Capture(...)` to intercept FFmpeg log lines. The captured delegate invokes `FfFilterGraphRunner.Execute(BuildInputs(), spec, FilterExecutionMode.DiscardOutput)`, so processing occurs without producing an output buffer. It returns the collected log entries as `IReadOnlyList<string>` for downstream parsing/inspection.


#### [[FfFilterGraph.CaptureLogs]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.CaptureLogs">
    <summary>
    Run the graph while capturing FFmpeg log output (via <see cref="T:Ams.Core.Services.Integrations.FFmpeg.FfLogCapture"/>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyList<string> CaptureLogs()
```

**Calls ->**
- [[FfFilterGraph.BuildInputs]]
- [[FfFilterGraph.BuildSpec]]
- [[FfFilterGraphRunner.Execute_2]]
- [[FfLogCapture.Capture]]

**Called-by <-**
- [[DspCommand.ExecuteFilterChain]]
- [[AudioProcessor.DetectSilence]]
- [[FfFilterGraph.Measure]]

