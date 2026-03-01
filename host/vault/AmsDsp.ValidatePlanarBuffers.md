---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs"
access_modifier: "private"
complexity: 10
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/validation
  - llm/error-handling
  - llm/utility
---
# AmsDsp::ValidatePlanarBuffers
**Path**: `Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs`

## Summary
**It validates planar float buffer shape and capacity against the configured channel count and requested frame count before native processing.**

`ValidatePlanarBuffers` is a precondition gate for the unsafe DSP path: it rejects null `input`/`output`, enforces `input.Length` and `output.Length` to exactly match `Channels`, then iterates each channel plane to ensure non-null arrays with `Length >= frames`. It throws `ArgumentNullException`, `ArgumentException`, or `ArgumentOutOfRangeException` with channel-specific messages, so `ProcessBlock` and `ProcessLong` can safely pin arrays and perform pointer arithmetic without per-iteration bounds/null checks.


#### [[AmsDsp.ValidatePlanarBuffers]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void ValidatePlanarBuffers(float[][] input, float[][] output, int frames)
```

**Called-by <-**
- [[AmsDsp.ProcessBlock]]
- [[AmsDsp.ProcessLong]]

