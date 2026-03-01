---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# FfFilterGraph::SerializeArguments
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.SerializeArguments]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string SerializeArguments(IEnumerable<(string Key, string Value)> kvPairs)
```

**Calls ->**
- [[FfFilterGraph.Escape]]

**Called-by <-**
- [[FfFilterGraph.AddFilter_2]]

