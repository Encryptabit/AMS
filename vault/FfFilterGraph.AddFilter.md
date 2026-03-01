---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 1
fan_in: 11
fan_out: 1
tags:
  - method
  - danger/high-fan-in
---
# FfFilterGraph::AddFilter
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

> [!danger] High Fan-In (11)
> This method is called by 11 other methods. Changes here have wide impact.


#### [[FfFilterGraph.AddFilter]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private FfFilterGraph AddFilter(string name, params (string Key, string Value)[] kv)
```

**Calls ->**
- [[FfFilterGraph.AddFilter_2]]

**Called-by <-**
- [[FfFilterGraph.ACompressor]]
- [[FfFilterGraph.ALimiter]]
- [[FfFilterGraph.ASetNSamples]]
- [[FfFilterGraph.AShowInfo]]
- [[FfFilterGraph.AspectralStats]]
- [[FfFilterGraph.DeEsser]]
- [[FfFilterGraph.FftDenoise]]
- [[FfFilterGraph.Gain]]
- [[FfFilterGraph.HighPass]]
- [[FfFilterGraph.LoudNorm]]
- [[FfFilterGraph.LowPass]]

