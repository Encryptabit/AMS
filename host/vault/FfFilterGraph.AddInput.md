---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 1
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
---
# FfFilterGraph::AddInput
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Registers an audio input buffer and its filter-graph label into the graph’s internal input list.**

`AddInput` is an expression-bodied helper that mutates internal graph state by appending a new `FfFilterGraphRunner.GraphInput` to `_inputs`. It constructs the wrapper as `new GraphInput(label, buffer)` and performs no null/format validation, deduplication, or side effects beyond list insertion. Because it is called from the constructor and `WithInput`, it centralizes input registration in one minimal path.


#### [[FfFilterGraph.AddInput]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void AddInput(AudioBuffer buffer, string label)
```

**Called-by <-**
- [[FfFilterGraph..ctor]]
- [[FfFilterGraph.WithInput]]

