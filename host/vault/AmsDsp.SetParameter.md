---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AmsDsp::SetParameter
**Path**: `Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs`

## Summary
**Sets a DSP parameter value (optionally at a sample offset) by validating initialization and delegating to the native AMS function.**

`SetParameter` is a thin managed interop wrapper in `Ams.Dsp.Native.AmsDsp` that performs an initialization guard via `EnsureInit()` before issuing the native call. After the guard, it forwards `id`, `value01`, and `sampleOffset` (default `0`) directly to `ams_set_parameter`. Its low complexity matches a straightforward precondition-check plus native dispatch path.


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

