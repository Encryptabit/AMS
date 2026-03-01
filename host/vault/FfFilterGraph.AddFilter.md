---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 1
fan_in: 11
fan_out: 1
tags:
  - method
  - danger/high-fan-in
  - llm/utility
  - llm/validation
---
# FfFilterGraph::AddFilter
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

> [!danger] High Fan-In (11)
> This method is called by 11 other methods. Changes here have wide impact.

## Summary
**Provide a convenient variadic entry point for adding a filter while routing to the main enumerable-based implementation.**

This `params` overload is a thin adapter that forwards tuple arguments to the enumerable-based `AddFilter` implementation. It converts the variadic `kv` array with `kv.AsEnumerable()` and computes `markFormatPinned` as `true` only when `name` equals `"aformat"` using `StringComparison.OrdinalIgnoreCase`. The method returns the delegated `FfFilterGraph` result to preserve fluent chaining.


#### [[FfFilterGraph.AddFilter]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private FfFilterGraph AddFilter(string name, params (string Key, string Value)[] kv)
```

**Calls ->**
- [[FfFilterGraph.AddFilter_2]]

**Called-by <-**
- [[FfFilterGraph.ACompressor]]
- [[FfFilterGraph.ALimiter]]
- [[FfFilterGraph.ASetNSamples]]
- [[FfFilterGraph.AShowInfo]]
- [[FfFilterGraph.AspectralStats]]
- [[FfFilterGraph.DeEsser]]
- [[FfFilterGraph.FftDenoise]]
- [[FfFilterGraph.Gain]]
- [[FfFilterGraph.HighPass]]
- [[FfFilterGraph.LoudNorm]]
- [[FfFilterGraph.LowPass]]

