---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "home/cari/repos/AMS/host/Ams.Dsp.Native/AmsDsp.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 2
tags:
  - method
---
# AmsDsp::SetParameter
**Path**: `home/cari/repos/AMS/host/Ams.Dsp.Native/AmsDsp.cs`


#### [[AmsDsp.SetParameter]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void SetParameter(uint id, float value01, uint sampleOffset = 0)
```

**Calls ->**
- [[AmsDsp.EnsureInit]]
- [[Native.ams_set_parameter]]

