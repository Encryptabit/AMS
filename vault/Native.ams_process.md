---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "home/cari/repos/AMS/host/Ams.Dsp.Native/Native.cs"
access_modifier: "public"
complexity: 1
fan_in: 2
fan_out: 0
tags:
  - method
---
# Native::ams_process
**Path**: `home/cari/repos/AMS/host/Ams.Dsp.Native/Native.cs`


#### [[Native.ams_process]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static extern void ams_process(float** in_ptrs, float** out_ptrs, uint nframes)
```

**Called-by <-**
- [[AmsDsp.ProcessBlock]]
- [[AmsDsp.ProcessLong]]

