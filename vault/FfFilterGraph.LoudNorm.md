---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
---
# FfFilterGraph::LoudNorm
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.LoudNorm]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph LoudNorm(LoudNormFilterParams parameters)
```

**Calls ->**
- [[FfFilterGraph.AddFilter]]
- [[FfFilterGraph.FormatDouble]]

**Called-by <-**
- [[FfFilterGraph.LoudNorm_2]]

