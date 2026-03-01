---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/entry-point
  - llm/utility
  - llm/error-handling
---
# FfFilterGraph::RunDiscardingOutput
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Run the composed filter graph against registered inputs while discarding produced audio output.**

`RunDiscardingOutput()` computes the active filter spec with `BuildSpec()` and executes it via `FfFilterGraphRunner.Execute(BuildInputs(), spec, FilterExecutionMode.DiscardOutput)`. It intentionally uses discard-output mode so the graph runs for side effects/metrics without materializing an audio buffer. Input presence validation is delegated to `BuildInputs()`.


#### [[FfFilterGraph.RunDiscardingOutput]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.RunDiscardingOutput">
    <summary>
    Execute the graph in discard-output mode (useful for measurement filters).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void RunDiscardingOutput()
```

**Calls ->**
- [[FfFilterGraph.BuildInputs]]
- [[FfFilterGraph.BuildSpec]]
- [[FfFilterGraphRunner.Execute_2]]

**Called-by <-**
- [[DspCommand.ExecuteFilterChain]]

