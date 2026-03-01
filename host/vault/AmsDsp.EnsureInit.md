---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs"
access_modifier: "private"
complexity: 2
fan_in: 6
fan_out: 0
tags:
  - method
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# AmsDsp::EnsureInit
**Path**: `Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs`

## Summary
**It validates that the wrapper has been initialized and throws a clear exception when operations are attempted in an invalid lifecycle state.**

`EnsureInit()` is a private guard method in `AmsDsp` that enforces object lifecycle state before any native DSP call path runs. Its implementation is a single check, `if (!_initialized)`, followed by `InvalidOperationException("AmsDsp not initialized. Use AmsDsp.Create(...)")`, and it is invoked at the start of `LoadState`, `ProcessBlock`, `ProcessLong`, `Reset`, `SaveState`, and `SetParameter`. This centralizes precondition enforcement so callers fail fast if `Create(...)` has not set `_initialized` to `true` (or after shutdown).


#### [[AmsDsp.EnsureInit]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void EnsureInit()
```

**Called-by <-**
- [[AmsDsp.LoadState]]
- [[AmsDsp.ProcessBlock]]
- [[AmsDsp.ProcessLong]]
- [[AmsDsp.Reset]]
- [[AmsDsp.SaveState]]
- [[AmsDsp.SetParameter]]

