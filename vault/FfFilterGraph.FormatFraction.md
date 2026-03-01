---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 1
fan_in: 3
fan_out: 1
tags:
  - method
---
# FfFilterGraph::FormatFraction
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.FormatFraction]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FormatFraction(double value)
```

**Calls ->**
- [[FfUtils.FormatFraction]]

**Called-by <-**
- [[FfFilterGraph.AspectralStats]]
- [[FfFilterGraph.DeEsser]]
- [[FfFilterGraph.DynaudNorm]]

