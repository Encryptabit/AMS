---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "home/cari/repos/AMS/host/Ams.Dsp.Native/AmsDsp.cs"
access_modifier: "public"
complexity: 7
fan_in: 0
fan_out: 3
tags:
  - method
---
# AmsDsp::ProcessLong
**Path**: `home/cari/repos/AMS/host/Ams.Dsp.Native/AmsDsp.cs`


#### [[AmsDsp.ProcessLong]]
##### What it does:
<!-- Badly formed XML comment ignored for member "M:Ams.Dsp.Native.AmsDsp.ProcessLong(System.Single[][],System.Single[][],System.Int32)" -->

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void ProcessLong(float[][] input, float[][] output, int totalFrames)
```

**Calls ->**
- [[AmsDsp.EnsureInit]]
- [[AmsDsp.ValidatePlanarBuffers]]
- [[Native.ams_process]]

