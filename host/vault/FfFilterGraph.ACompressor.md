---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
---
# FfFilterGraph::ACompressor
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Construct and add an `acompressor` filter node from a parameter object with properly formatted numeric values.**

This overload materializes compressor arguments and appends them with `AddFilter("acompressor", ...)`. It handles null input by falling back to `new ACompressorFilterParams()`, formats threshold/makeup via `FormatDecibels`, and serializes ratio/attack/release with `FormatDouble` before emitting FFmpeg option keys (`threshold`, `ratio`, `attack`, `release`, `makeup`). It returns the same `FfFilterGraph` for fluent chaining.


#### [[FfFilterGraph.ACompressor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph ACompressor(ACompressorFilterParams parameters)
```

**Calls ->**
- [[FfFilterGraph.AddFilter]]
- [[FfFilterGraph.FormatDecibels]]
- [[FfFilterGraph.FormatDouble]]

**Called-by <-**
- [[FfFilterGraph.ACompressor_2]]

