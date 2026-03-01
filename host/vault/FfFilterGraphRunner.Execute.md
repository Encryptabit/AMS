---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/entry-point
  - llm/utility
---
# FfFilterGraphRunner::Execute
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Runs a filter graph for one input buffer in the specified execution mode without exposing a direct return value.**

`Execute(AudioBuffer input, string filterSpec, FilterExecutionMode mode)` is a single-input wrapper that creates a one-element input array labeled `"main"` and forwards it to `ExecuteInternal(inputs, filterSpec, mode, null)`. It does not return output and performs no additional validation or exception translation beyond what `ExecuteInternal` enforces.


#### [[FfFilterGraphRunner.Execute]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void Execute(AudioBuffer input, string filterSpec, FfFilterGraphRunner.FilterExecutionMode mode)
```

**Calls ->**
- [[FfFilterGraphRunner.ExecuteInternal]]

