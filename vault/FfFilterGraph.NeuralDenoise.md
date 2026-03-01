---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 5
tags:
  - method
---
# FfFilterGraph::NeuralDenoise
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.NeuralDenoise]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FfFilterGraph NeuralDenoise(NeuralDenoiseFilterParams parameters)
```

**Calls ->**
- [[FfFilterGraph.AddRawFilter]]
- [[FfFilterGraph.FormatDouble]]
- [[FfFilterGraph.FormatFilterPathArgument]]
- [[FfFilterGraph.NormalizeFilterPathArgument]]
- [[FfFilterGraph.ResolveFilterAssetPath]]

**Called-by <-**
- [[FfFilterGraph.NeuralDenoise_2]]

