---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "home/cari/repos/AMS/host/Ams.Dsp.Native/AmsDsp.cs"
access_modifier: "public"
complexity: 6
fan_in: 0
fan_out: 3
tags:
  - method
---
# AmsDsp::ProcessBlock
**Path**: `home/cari/repos/AMS/host/Ams.Dsp.Native/AmsDsp.cs`


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

