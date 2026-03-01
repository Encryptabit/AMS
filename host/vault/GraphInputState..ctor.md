---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
---
# GraphInputState::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Packages an input’s label, audio buffer metadata, FFmpeg source context, channel layout, and reusable frame pointer into a single execution-state object.**

The `GraphInputState` constructor is a direct state-initialization routine that captures source wiring and reusable frame resources for one graph input. It assigns `Label`, `Buffer`, `Source`, `Layout`, and `Frame`, and derives `SampleRate` and `Channels` from the provided `AudioBuffer` rather than separate arguments. The constructor performs no validation or allocation, so pointer/object validity is assumed to be enforced by the caller.


#### [[GraphInputState..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public GraphInputState(string label, AudioBuffer buffer, AVFilterContext* source, AVChannelLayout layout, AVFrame* frame)
```

