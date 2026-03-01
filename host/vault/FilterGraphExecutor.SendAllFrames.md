---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
---
# FilterGraphExecutor::SendAllFrames
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Streams an input buffer into the graph in fixed-size sample chunks while interleaving output draining and honoring early-stop signals.**

`SendAllFrames` chunk-feeds one `GraphInputState` buffer into the filter graph using a loop over `state.Buffer.Length`. It computes `take = Math.Min(ChunkSamples, total - offset)`, calls `SendFrame(state, offset, take, pts)`, and stops early when `SendFrame` returns `false` (e.g., downstream EOF such as `atrim` saturation). After each successful send it advances `pts`/`offset` by `take` and calls `Drain()` to pull available output incrementally.


#### [[FilterGraphExecutor.SendAllFrames]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void SendAllFrames(FfFilterGraphRunner.GraphInputState state)
```

**Calls ->**
- [[FilterGraphExecutor.Drain]]
- [[FilterGraphExecutor.SendFrame]]

**Called-by <-**
- [[FilterGraphExecutor.Process]]

