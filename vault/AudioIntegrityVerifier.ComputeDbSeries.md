---
namespace: "Ams.Core.Audio"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
---
# AudioIntegrityVerifier::ComputeDbSeries
**Path**: `home/cari/repos/AMS/host/Ams.Core/Audio/AudioIntegrityVerifier.cs`


#### [[AudioIntegrityVerifier.ComputeDbSeries]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static double[] ComputeDbSeries(float[] samples, int sr, double windowSec, double stepSec)
```

**Called-by <-**
- [[AudioIntegrityVerifier.Verify]]

