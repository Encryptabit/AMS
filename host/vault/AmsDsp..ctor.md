---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs"
access_modifier: "private"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/factory
  - llm/utility
---
# AmsDsp::.ctor
**Path**: `Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs`

## Summary
**Construct an `AmsDsp` object with immutable DSP configuration values that were already validated and initialized by the factory path.**

This private constructor in `Ams.Dsp.Native.AmsDsp` is intentionally trivial (complexity 1): it assigns `sampleRate`, `maxBlock`, and `channels` directly into the backing readonly fields `_sampleRate`, `_maxBlock`, and `_channels`. It does not validate inputs, call native APIs, or set `_initialized`; those concerns are handled by `Create(...)` before/after constructing the instance.


#### [[AmsDsp..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private AmsDsp(float sampleRate, uint maxBlock, uint channels)
```

