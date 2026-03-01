---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 5
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfFilterGraph::NeuralDenoise
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Construct and add a neural denoise filter with validated model path and bounded mix settings.**

This overload builds the `arnndn` filter argument string from a parameter object and appends it via `AddRawFilter("arnndn", rawArgs)`. It is null-tolerant (`parameters ?? new NeuralDenoiseFilterParams()`), resolves and normalizes the model path through `ResolveFilterAssetPath` and `NormalizeFilterPathArgument`, then escapes it with `FormatFilterPathArgument` for FFmpeg-safe usage. The `Mix` value is clamped to `[0,1]` and formatted with `FormatDouble` before composing `model=...:mix=...`.


#### [[FfFilterGraph.NeuralDenoise]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph NeuralDenoise(NeuralDenoiseFilterParams parameters)
```

**Calls ->**
- [[FfFilterGraph.AddRawFilter]]
- [[FfFilterGraph.FormatDouble]]
- [[FfFilterGraph.FormatFilterPathArgument]]
- [[FfFilterGraph.NormalizeFilterPathArgument]]
- [[FfFilterGraph.ResolveFilterAssetPath]]

**Called-by <-**
- [[FfFilterGraph.NeuralDenoise_2]]

