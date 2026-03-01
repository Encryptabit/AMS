---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs"
access_modifier: "private"
complexity: 6
fan_in: 2
fan_out: 0
tags:
  - method
---
# FfDecoder::ReadTags
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs`


#### [[FfDecoder.ReadTags]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyDictionary<string, string> ReadTags(AVDictionary* metadata)
```

**Called-by <-**
- [[FfDecoder.Decode]]
- [[FfDecoder.Probe]]

