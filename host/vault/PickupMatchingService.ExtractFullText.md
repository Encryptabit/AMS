---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs"
access_modifier: "internal"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
---
# PickupMatchingService::ExtractFullText
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PickupMatchingService.cs`


#### [[PickupMatchingService.ExtractFullText]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PickupMatchingService.ExtractFullText(Ams.Core.Asr.AsrResponse)">
    <summary>
    Extracts the full recognized text from an ASR response by joining token words.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static string ExtractFullText(AsrResponse response)
```

**Called-by <-**
- [[PickupMatchingService.MatchSinglePickupAsync]]

