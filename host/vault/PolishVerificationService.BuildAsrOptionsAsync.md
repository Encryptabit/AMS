---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PolishVerificationService.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
---
# PolishVerificationService::BuildAsrOptionsAsync
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PolishVerificationService.cs`


#### [[PolishVerificationService.BuildAsrOptionsAsync]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PolishVerificationService.BuildAsrOptionsAsync(System.Threading.CancellationToken)">
    <summary>
    Builds default ASR options, resolving the Whisper model path with auto-download fallback.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<AsrOptions> BuildAsrOptionsAsync(CancellationToken ct)
```

**Calls ->**
- [[AsrEngineConfig.ResolveModelPathAsync]]

**Called-by <-**
- [[PolishVerificationService.RevalidateSegmentAsync]]

