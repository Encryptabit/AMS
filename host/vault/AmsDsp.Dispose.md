---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
---
# AmsDsp::Dispose
**Path**: `Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs`

## Summary
**Release native DSP resources owned by `AmsDsp` by shutting down the underlying AMS runtime.**

The `Ams.Dsp.Native.AmsDsp.Dispose()` method performs deterministic teardown of the native DSP runtime by invoking `ams_shutdown`. Its low complexity (2) indicates minimal control flow, likely just a lightweight guard around shutdown behavior. This method represents the managed-to-native cleanup boundary for releasing unmanaged DSP state.


#### [[AmsDsp.Dispose]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Dispose()
```

**Calls ->**
- [[Native.ams_shutdown]]

