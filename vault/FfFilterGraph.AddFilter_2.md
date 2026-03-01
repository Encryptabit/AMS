---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 4
fan_in: 3
fan_out: 1
tags:
  - method
---
# FfFilterGraph::AddFilter
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.AddFilter_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private FfFilterGraph AddFilter(string name, IEnumerable<(string Key, string Value)> kvPairs, bool markFormatPinned = false)
```

**Calls ->**
- [[FfFilterGraph.SerializeArguments]]

**Called-by <-**
- [[FfFilterGraph.AddFilter]]
- [[FfFilterGraph.AFormat]]
- [[FfFilterGraph.DynaudNorm]]

