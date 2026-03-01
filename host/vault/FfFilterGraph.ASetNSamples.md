---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# FfFilterGraph::ASetNSamples
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Add a fixed-window `asetnsamples` filter with minimum-safe sample count and configurable padding behavior.**

This method appends an `asetnsamples` filter with options built via `AddFilter("asetnsamples", ...)`. It enforces a minimum sample window size by clamping `sampleCount` to at least `1` (`Math.Max(sampleCount, 1)`), formats the numeric value with `FormatDouble`, and emits `pad` as `"1"` or `"0"` based on `padIncompleteWindows` (default `true`). The return value is the current `FfFilterGraph` for fluent chaining.


#### [[FfFilterGraph.ASetNSamples]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.ASetNSamples(System.Int32,System.Boolean)">
    <summary>
    Enforce fixed-size analysis windows (libavfilter <c>asetnsamples</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph ASetNSamples(int sampleCount, bool padIncompleteWindows = true)
```

**Calls ->**
- [[FfFilterGraph.AddFilter]]
- [[FfFilterGraph.FormatDouble]]

