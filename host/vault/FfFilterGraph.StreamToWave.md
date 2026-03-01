---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 4
tags:
  - method
  - llm/entry-point
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfFilterGraph::StreamToWave
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Run the filter graph and write the encoded audio result directly to an output stream.**

`StreamToWave` executes the composed graph and streams encoded output into a caller-provided `Stream`. It validates `output` via `ArgumentNullException.ThrowIfNull`, builds the filter spec (`BuildSpec()`), creates a streaming encoder sink with either provided options or `new AudioEncodeOptions()`, and invokes `FfFilterGraphRunner.Stream(BuildInputs(), spec, sink)`. The `using` scope ensures sink disposal after streaming completes.


#### [[FfFilterGraph.StreamToWave]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void StreamToWave(Stream output, AudioEncodeOptions options = null)
```

**Calls ->**
- [[FfEncoder.CreateStreamingSink]]
- [[FfFilterGraph.BuildInputs]]
- [[FfFilterGraph.BuildSpec]]
- [[FfFilterGraphRunner.Stream]]

**Called-by <-**
- [[DspCommand.ExecuteFilterChain]]

