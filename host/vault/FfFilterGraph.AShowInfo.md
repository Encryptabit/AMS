---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# FfFilterGraph::AShowInfo
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Add the `ashowinfo` debug filter, optionally with a configured log level argument.**

This method conditionally emits an `ashowinfo` filter clause based on whether `level` is provided. If `level` is null/whitespace, it calls `AddFilter("ashowinfo")`; otherwise it calls `AddFilter("ashowinfo", ("level", level))` to include FFmpeg’s level option. The branching keeps default behavior minimal while allowing explicit verbosity control.


#### [[FfFilterGraph.AShowInfo]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.AShowInfo(System.String)">
    <summary>
    Emit per-frame debug info (libavfilter <c>ashowinfo</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph AShowInfo(string level = null)
```

**Calls ->**
- [[FfFilterGraph.AddFilter]]

