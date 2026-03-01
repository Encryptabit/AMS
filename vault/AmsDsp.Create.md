---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "home/cari/repos/AMS/host/Ams.Dsp.Native/AmsDsp.cs"
access_modifier: "public"
complexity: 7
fan_in: 0
fan_out: 2
tags:
  - method
---
# AmsDsp::Create
**Path**: `home/cari/repos/AMS/host/Ams.Dsp.Native/AmsDsp.cs`


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

