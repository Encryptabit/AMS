---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/factory
---
# FfFilterGraph::DeEsser
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Add a de-esser filter to `FfFilterGraph` using simple scalar arguments.**

This expression-bodied overload is a convenience entry that constructs `new DeEsserFilterParams(normalizedFrequency, intensity, maxReduction, outputMode)` and delegates directly to `DeEsser(DeEsserFilterParams?)`. Defaults (`0.5`, `0`, `0.5`, `"o"`) keep call sites terse and align with typical de-esser usage. Argument normalization/serialization and actual filter insertion are handled by the delegated overload.


#### [[FfFilterGraph.DeEsser_2]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.DeEsser(System.Double,System.Double,System.Double,System.String)">
    <summary>
    Simple de-esser (libavfilter <c>deesser</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph DeEsser(double normalizedFrequency = 0.5, double intensity = 0, double maxReduction = 0.5, string outputMode = "o")
```

**Calls ->**
- [[FfFilterGraph.DeEsser]]

