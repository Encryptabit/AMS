---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs"
access_modifier: "public"
complexity: 19
fan_in: 1
fan_out: 4
tags:
  - method
  - danger/high-complexity
---
# FfDecoder::Probe
**Path**: `home/cari/repos/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs`

> [!danger] High Complexity (19)
> Cyclomatic complexity: 19. Consider refactoring into smaller methods.


#### [[FfDecoder.Probe]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static AudioInfo Probe(string path)
```

**Calls ->**
- [[FfDecoder.PtrToStringUtf8]]
- [[FfDecoder.ReadTags]]
- [[FfSession.EnsureInitialized]]
- [[FfUtils.ThrowIfError]]

**Called-by <-**
- [[AudioProcessor.Probe]]

