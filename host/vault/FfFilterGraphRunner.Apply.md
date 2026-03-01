---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/entry-point
  - llm/utility
  - llm/error-handling
---
# FfFilterGraphRunner::Apply
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs`

## Summary
**Runs an FFmpeg filter specification against one audio buffer and returns the produced audio output.**

`Apply(AudioBuffer input, string filterSpec)` is a convenience entry point for single-input filter execution that wraps the input as `new GraphInput("main", input)`. It delegates to `ExecuteInternal(inputs, filterSpec, FilterExecutionMode.ReturnAudio, null)` and enforces a non-null result with a null-coalescing throw (`InvalidOperationException`) if the filter graph produces no output buffer.


#### [[FfFilterGraphRunner.Apply]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioBuffer Apply(AudioBuffer input, string filterSpec)
```

**Calls ->**
- [[FfFilterGraphRunner.ExecuteInternal]]

