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
---
# FfFilterGraph::SilenceRemove
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Add a silence-trimming filter using a caller-supplied or default raw FFmpeg argument string.**

This expression-bodied overload directly appends FFmpeg’s `silenceremove` filter by forwarding a raw argument string to `AddRawFilter("silenceremove", args)`. It provides a fully populated default argument set for start/stop periods and thresholds (`start_periods=0:start_threshold=-50dB:stop_periods=0:stop_threshold=-50dB`), allowing immediate use without parameter-object construction. No parsing or normalization is performed in this method.


#### [[FfFilterGraph.SilenceRemove_2]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.SilenceRemove(System.String)">
    <summary>
    Silence trimming (libavfilter <c>silenceremove</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph SilenceRemove(string args = "start_periods=0:start_threshold=-50dB:stop_periods=0:stop_threshold=-50dB")
```

**Calls ->**
- [[FfFilterGraph.AddRawFilter]]

