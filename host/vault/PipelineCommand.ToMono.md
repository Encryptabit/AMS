---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# PipelineCommand::ToMono
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Converts a planar `AudioBuffer` to mono samples by averaging all channels (or cloning the single channel) without mutating the source.**

`ToMono` produces a standalone mono `float[]` from an `AudioBuffer` with planar channel data. If `Channels == 1`, it returns a clone of `buffer.Planar[0]`; otherwise it allocates `new float[buffer.Length]`, computes `scale = 1f / buffer.Channels`, and uses nested channel/sample loops to accumulate `mono[i] += src[i] * scale`, effectively averaging channels per frame.


#### [[PipelineCommand.ToMono]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static float[] ToMono(AudioBuffer buffer)
```

**Called-by <-**
- [[PipelineCommand.RunVerify]]

