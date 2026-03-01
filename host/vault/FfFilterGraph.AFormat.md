---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# FfFilterGraph::AFormat
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Adds an explicit `aformat` stage to the fluent audio filter graph to normalize downstream format/layout/rate and mark the graph’s format as pinned.**

`AFormat` constructs an argument list for FFmpeg’s `aformat` filter by always seeding `sample_fmts` from `sampleFormats`, then conditionally adding `channel_layouts` (only when non-whitespace) and `sample_rates` (only when `sampleRate.HasValue`, formatted via `FormatDouble`). It delegates to `AddFilter("aformat", args, markFormatPinned: true)`, so argument serialization will skip null/blank values and escape accepted values before emitting the clause. Setting `markFormatPinned` also flips `_formatPinned`, preventing `EnsureDefaultFormatClause()` from injecting the default `aformat=sample_fmts=flt` later.


#### [[FfFilterGraph.AFormat]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.AFormat(System.String,System.String,System.Nullable{System.Int32})">
    <summary>
    Ensure downstream filters see a consistent format/layout.
    Uses libavfilter's <c>aformat</c> (ffmpeg <c>-af aformat</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph AFormat(string sampleFormats = "flt", string channelLayouts = null, int? sampleRate = null)
```

**Calls ->**
- [[FfFilterGraph.AddFilter_2]]
- [[FfFilterGraph.FormatDouble]]

