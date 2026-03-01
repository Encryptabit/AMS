---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/factory
  - llm/validation
  - llm/utility
---
# FfFilterGraph::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Initializes a filter graph instance with its first audio input and active input label.**

This private constructor seeds a new filter-graph builder with a required primary input buffer and label state. It validates `buffer` (`ThrowIfNull`), normalizes blank labels to `"main"`, sets `_inputLabel`, and immediately registers the input via `AddInput(buffer, label)` so subsequent fluent clauses have a bound source. It establishes initial graph state (`_inputs` + active label) in one step.


#### [[FfFilterGraph..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private FfFilterGraph(AudioBuffer buffer, string label)
```

**Calls ->**
- [[FfFilterGraph.AddInput]]

