---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 0
tags:
  - method
---
# FfDecoder::PtrToStringUtf8
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs`


#### [[FfDecoder.PtrToStringUtf8]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string PtrToStringUtf8(byte* value)
```

**Called-by <-**
- [[FfDecoder.Decode]]
- [[FfDecoder.Probe]]

