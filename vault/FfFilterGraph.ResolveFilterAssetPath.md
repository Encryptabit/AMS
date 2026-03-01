---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 2
tags:
  - method
---
# FfFilterGraph::ResolveFilterAssetPath
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraph.cs`


#### [[FfFilterGraph.ResolveFilterAssetPath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolveFilterAssetPath(string model)
```

**Calls ->**
- [[FfFilterGraph.CopyFilterAssetToWorkingDirectory]]
- [[FfFilterGraph.TryGetRelativePathSafe]]

**Called-by <-**
- [[FfFilterGraph.NeuralDenoise]]

