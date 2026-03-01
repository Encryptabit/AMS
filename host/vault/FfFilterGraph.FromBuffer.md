---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 2
fan_in: 6
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/entry-point
  - llm/utility
---
# FfFilterGraph::FromBuffer
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Creates a new filter-graph builder rooted at one audio buffer input with an optional label.**

This static factory starts a fluent `FfFilterGraph` from a single input buffer. It instantiates the graph via `new(buffer, label ?? "main")`, defaulting the label when omitted/null, which in turn registers the input and sets active input state. The method provides the canonical entry for graph construction used by downstream filter operations.


#### [[FfFilterGraph.FromBuffer]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.FromBuffer(Ams.Core.Artifacts.AudioBuffer,System.String)">
    <summary>
    Begin a fluent graph with a single <see cref="T:Ams.Core.Artifacts.AudioBuffer"/>.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static FfFilterGraph FromBuffer(AudioBuffer buffer, string label = null)
```

**Called-by <-**
- [[DspCommand.BuildFilterGraph]]
- [[AsrAudioPreparer.DownmixToMono]]
- [[AudioProcessor.DetectSilence]]
- [[AudioProcessor.FadeIn]]
- [[AudioProcessor.Resample]]
- [[AudioProcessor.Trim]]

