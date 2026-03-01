---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 1
fan_in: 12
fan_out: 1
tags:
  - method
  - danger/high-fan-in
  - llm/utility
---
# FfFilterGraph::FormatDouble
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

> [!danger] High Fan-In (12)
> This method is called by 12 other methods. Changes here have wide impact.

## Summary
**Converts a `double` into the project’s standardized FFmpeg-compatible numeric string representation.**

`FormatDouble` is an expression-bodied adapter that delegates directly to `FfUtils.FormatNumber(value)` to produce a canonical numeric string for FFmpeg argument emission. It adds no local logic, state mutation, or error handling, and exists as a class-local formatting abstraction used across many filter builders.


#### [[FfFilterGraph.FormatDouble]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FormatDouble(double value)
```

**Calls ->**
- [[FfUtils.FormatNumber]]

**Called-by <-**
- [[FfFilterGraph.ACompressor]]
- [[FfFilterGraph.AFormat]]
- [[FfFilterGraph.ALimiter]]
- [[FfFilterGraph.ASetNSamples]]
- [[FfFilterGraph.AspectralStats]]
- [[FfFilterGraph.DynaudNorm]]
- [[FfFilterGraph.FftDenoise]]
- [[FfFilterGraph.Gain]]
- [[FfFilterGraph.HighPass]]
- [[FfFilterGraph.LoudNorm]]
- [[FfFilterGraph.LowPass]]
- [[FfFilterGraph.NeuralDenoise]]

