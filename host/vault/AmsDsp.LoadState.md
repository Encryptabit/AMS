---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# AmsDsp::LoadState
**Path**: `Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs`

## Summary
**Loads serialized DSP state bytes into the native AMS DSP engine after verifying the instance is initialized.**

`LoadState` is a thin unsafe interop wrapper: it first calls `EnsureInit()` to guard against use before initialization, then pins the incoming `ReadOnlySpan<byte>` with `fixed (byte* p = state)`. It forwards the pinned pointer and `state.Length` cast to `UIntPtr` into `Native.ams_load_state(p, (UIntPtr)state.Length)`, performing no additional transformation or copy.


#### [[AmsDsp.LoadState]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void LoadState(ReadOnlySpan<byte> state)
```

**Calls ->**
- [[AmsDsp.EnsureInit]]
- [[Native.ams_load_state]]

