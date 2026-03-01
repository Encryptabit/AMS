---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
---
# FfFilterGraph::Measure
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.Measure]]
##### What it does:
<member name="M:Ams.Core.Services.Integrations.FFmpeg.FfFilterGraph.Measure``1(System.Func{System.Collections.Generic.IReadOnlyList{System.String},``0})">
    <summary>
    Execute the graph in measurement mode and parse the collected logs.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public T Measure<T>(Func<IReadOnlyList<string>, T> parser)
```

**Calls ->**
- [[FfFilterGraph.CaptureLogs]]

