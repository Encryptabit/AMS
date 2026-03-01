---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs"
access_modifier: "public"
complexity: 7
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/factory
  - llm/validation
  - llm/error-handling
---
# AmsDsp::Create
**Path**: `Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs`

## Summary
**Creates and returns an initialized `AmsDsp` instance after validating arguments, checking native ABI compatibility, and ensuring native initialization succeeds.**

`Create` performs strict parameter validation (`sampleRate > 0`, `maxBlock > 0`, `channels` in `1..8`) and throws `ArgumentOutOfRangeException` on invalid input. It calls `Native.ams_get_abi(out major, out minor)` and enforces `major == ExpectedAbiMajor`, raising `NotSupportedException` with expected/reported ABI values when incompatible. It then invokes `Native.ams_init(sampleRate, maxBlock, channels)`, throws `InvalidOperationException` on non-zero return code, and returns a new `AmsDsp` via the private constructor with `_initialized` set to `true`.


#### [[AmsDsp.Create]]
##### What it does:
<member name="M:Ams.Dsp.Native.AmsDsp.Create(System.Single,System.UInt32,System.UInt32)">
    <summary>Factory that checks ABI and initializes native state once.</summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AmsDsp Create(float sampleRate, uint maxBlock, uint channels)
```

**Calls ->**
- [[Native.ams_get_abi]]
- [[Native.ams_init]]

