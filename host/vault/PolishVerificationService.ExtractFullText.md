---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PolishVerificationService.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
---
# PolishVerificationService::ExtractFullText
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PolishVerificationService.cs`


#### [[PolishVerificationService.ExtractFullText]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PolishVerificationService.ExtractFullText(Ams.Core.Asr.AsrResponse)">
    <summary>
    Extracts the full recognized text from an ASR response by joining token words.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ExtractFullText(AsrResponse response)
```

**Called-by <-**
- [[PolishVerificationService.RevalidateSegmentAsync]]

