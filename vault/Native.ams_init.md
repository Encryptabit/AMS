---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "home/cari/repos/AMS/host/Ams.Dsp.Native/Native.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
---
# Native::ams_init
**Path**: `home/cari/repos/AMS/host/Ams.Dsp.Native/Native.cs`


#### [[Native.ams_init]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static extern int ams_init(float sample_rate, uint max_block, uint channels)
```

**Called-by <-**
- [[AmsDsp.Create]]

