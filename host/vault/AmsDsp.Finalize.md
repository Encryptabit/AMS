---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs"
access_modifier: "protected"
complexity: 3
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
---
# AmsDsp::Finalize
**Path**: `Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs`

## Summary
**It shuts down the AMS native DSP subsystem when the `AmsDsp` instance is being destroyed/finalized.**

`~AmsDsp` in `Ams.Dsp.Native.AmsDsp` is a finalization/destruction hook that delegates cleanup to the native `ams_shutdown` call. The method is intentionally small (complexity 3), indicating limited control flow and a narrow responsibility focused on unmanaged teardown. It serves as lifecycle glue between the managed wrapper and native DSP runtime shutdown.


#### [[AmsDsp.Finalize]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
protected ~AmsDsp()
```

**Calls ->**
- [[Native.ams_shutdown]]

