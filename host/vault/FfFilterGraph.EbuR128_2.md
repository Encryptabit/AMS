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
# FfFilterGraph::EbuR128
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

## Summary
**Add an EBU R128 measurement filter using caller-provided or default raw FFmpeg arguments.**

This expression-bodied overload appends FFmpeg’s `ebur128` loudness-measurement filter by forwarding a raw option string to `AddRawFilter("ebur128", args)`. It defaults to `framelog=verbose`, enabling detailed frame-level logging without additional setup. The method performs no local parsing or validation of the provided argument string.


#### [[FfFilterGraph.EbuR128_2]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.EbuR128(System.String)">
    <summary>
    Measurement helper (libavfilter <c>ebur128</c>).
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph EbuR128(string args = "framelog=verbose")
```

**Calls ->**
- [[FfFilterGraph.AddRawFilter]]

