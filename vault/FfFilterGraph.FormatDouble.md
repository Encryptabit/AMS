---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 1
fan_in: 12
fan_out: 1
tags:
  - method
  - danger/high-fan-in
---
# FfFilterGraph::FormatDouble
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`

> [!danger] High Fan-In (12)
> This method is called by 12 other methods. Changes here have wide impact.


#### [[FfFilterGraph.FormatDouble]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string FormatDouble(double value)
```

**Calls ->**
- [[FfUtils.FormatNumber]]

**Called-by <-**
- [[FfFilterGraph.ACompressor]]
- [[FfFilterGraph.AFormat]]
- [[FfFilterGraph.ALimiter]]
- [[FfFilterGraph.ASetNSamples]]
- [[FfFilterGraph.AspectralStats]]
- [[FfFilterGraph.DynaudNorm]]
- [[FfFilterGraph.FftDenoise]]
- [[FfFilterGraph.Gain]]
- [[FfFilterGraph.HighPass]]
- [[FfFilterGraph.LoudNorm]]
- [[FfFilterGraph.LowPass]]
- [[FfFilterGraph.NeuralDenoise]]

