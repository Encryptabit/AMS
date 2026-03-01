---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 1
fan_in: 4
fan_out: 3
tags:
  - method
  - llm/entry-point
  - llm/utility
  - llm/error-handling
---
# FfFilterGraph::ToBuffer
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Execute the currently composed FFmpeg filter graph against registered inputs and return the processed audio as an `AudioBuffer`.**

`ToBuffer()` materializes the current graph spec by calling `BuildSpec()`, then executes the graph in one step through `FfFilterGraphRunner.Apply(BuildInputs(), spec)`. The method delegates input validation to `BuildInputs()` (which enforces at least one registered source) and returns the newly produced `AudioBuffer` from the runner call. Its implementation is a thin synchronous execution entry point over the composed fluent graph state.


#### [[FfFilterGraph.ToBuffer]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.ToBuffer">
    <summary>
    Execute the composed graph and return a new <see cref="T:Ams.Core.Artifacts.AudioBuffer"/>.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AudioBuffer ToBuffer()
```

**Calls ->**
- [[FfFilterGraph.BuildInputs]]
- [[FfFilterGraph.BuildSpec]]
- [[FfFilterGraphRunner.Apply_2]]

**Called-by <-**
- [[AsrAudioPreparer.DownmixToMono]]
- [[AudioProcessor.FadeIn]]
- [[AudioProcessor.Resample]]
- [[AudioProcessor.Trim]]

