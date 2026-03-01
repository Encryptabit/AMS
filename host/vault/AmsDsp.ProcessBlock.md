---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs"
access_modifier: "public"
complexity: 6
fan_in: 0
fan_out: 3
tags:
  - method
  - llm/entry-point
  - llm/validation
  - llm/error-handling
---
# AmsDsp::ProcessBlock
**Path**: `Projects/AMS/host/Ams.Dsp.Native/AmsDsp.cs`

## Summary
**Processes a single planar float32 audio block of `nframes` by validating channel buffers, pinning managed arrays, and forwarding native channel pointers to the DSP engine.**

`ProcessBlock` first enforces object state and buffer contracts by calling `EnsureInit()` and `ValidatePlanarBuffers(input, output, nframes)`, then processes exactly one block across `Channels`. It allocates per-channel `GCHandle` arrays, pins each input/output plane, stack-allocates `IntPtr*` channel pointer tables, fills them from `AddrOfPinnedObject`, and calls `Native.ams_process((float**)inPtrs, (float**)outPtrs, checked((uint)nframes))`. Pin lifetimes are wrapped in `try/finally` so all handles are freed on both success and failure, while invalid state/arguments are surfaced as exceptions from the guard methods.


#### [[AmsDsp.ProcessBlock]]
##### What it does:
<member name="M:Ams.Dsp.Native.AmsDsp.ProcessBlock(System.Single[][],System.Single[][],System.Int32)">
    <summary>
    Process exactly <paramref name="nframes"/> frames in one call.
    <para>Buffer format: PLANAR float32. in[ch].Length and out[ch].Length MUST be >= nframes.</para>
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void ProcessBlock(float[][] input, float[][] output, int nframes)
```

**Calls ->**
- [[AmsDsp.EnsureInit]]
- [[AmsDsp.ValidatePlanarBuffers]]
- [[Native.ams_process]]

