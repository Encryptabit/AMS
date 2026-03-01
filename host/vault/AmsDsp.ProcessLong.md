---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs"
access_modifier: "public"
complexity: 7
fan_in: 0
fan_out: 3
tags:
  - method
  - llm/entry-point
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AmsDsp::ProcessLong
**Path**: `Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs`

## Summary
**It validates DSP initialization and planar buffer arguments, then invokes native processing for the requested frame count.**

`ProcessLong` is a public managed wrapper in `Ams.Dsp.Native.AmsDsp` that executes a long-form DSP pass over planar `float[][]` input/output buffers. It performs a strict call sequence of `EnsureInit` (state readiness), `ValidatePlanarBuffers` (buffer/frame contract checks), then `ams_process` (native execution) using `totalFrames` as the processing span. Given the `void` signature and reported complexity of 7, control flow is primarily guard-driven, with invalid state/arguments handled through validation/exception paths rather than return codes.


#### [[AmsDsp.ProcessLong]]
##### What it does:
<!-- Badly formed XML comment ignored for member "M:Ams.Dsp.Native.AmsDsp.ProcessLong(System.Single[][],System.Single[][],System.Int32)" -->

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void ProcessLong(float[][] input, float[][] output, int totalFrames)
```

**Calls ->**
- [[AmsDsp.EnsureInit]]
- [[AmsDsp.ValidatePlanarBuffers]]
- [[Native.ams_process]]

