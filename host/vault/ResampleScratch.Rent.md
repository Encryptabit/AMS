---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ResampleScratch::Rent
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs`

## Summary
**Allocates or reuses native FFmpeg sample buffers sized for the requested resample output layout.**

This method provides reusable unmanaged sample buffers for resampling output, reallocating only when shape or format requirements change. If existing buffers are missing or incompatible (`channels`, `samples` capacity, or `format` mismatch), it calls `Release()`, updates cached shape fields, then allocates fresh FFmpeg buffers via `av_samples_alloc_array_and_samples` and validates the result with `ThrowIfError`. It returns the cached `byte**` buffer pointer for direct use by `swr_convert`.


#### [[ResampleScratch.Rent]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public byte** Rent(int channels, int samples, AVSampleFormat format)
```

**Calls ->**
- [[ResampleScratch.Release]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[FfDecoder.ResampleInto]]

