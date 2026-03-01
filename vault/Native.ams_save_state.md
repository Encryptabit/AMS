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
# Native::ams_save_state
**Path**: `home/cari/repos/AMS/host/Ams.Dsp.Native/Native.cs`


#### [[Native.ams_save_state]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static extern void ams_save_state(byte* buf, ref nuint inout_len)
```

**Called-by <-**
- [[AmsDsp.SaveState]]

