---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 2
tags:
  - method
---
# ResampleScratch::Rent
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs`


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

