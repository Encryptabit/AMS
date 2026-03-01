---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PickupMfaRefinementService.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 2
tags:
  - method
---
# PickupMfaRefinementService::BuildAlignmentWords
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PickupMfaRefinementService.cs`


#### [[PickupMfaRefinementService.BuildAlignmentWords]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (List<string> Words, List<int> WordToTokenIndex) BuildAlignmentWords(IReadOnlyList<AsrToken> tokens)
```

**Calls ->**
- [[PronunciationHelper.ExtractPronunciationParts]]
- [[PickupMfaRefinementService.NormalizeAlignmentWord]]

**Called-by <-**
- [[PickupMfaRefinementService.RefineAsrTimingsAsync]]

