---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaService.cs"
access_modifier: "public"
complexity: 5
fan_in: 3
fan_out: 3
tags:
  - method
---
# MfaService::GeneratePronunciationsAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaService.cs`


#### [[MfaService.GeneratePronunciationsAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<MfaCommandResult> GeneratePronunciationsAsync(MfaChapterContext context, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[MfaService.Quote]]
- [[MfaService.QuoteRequired]]
- [[MfaService.RunCommandAsync]]

**Called-by <-**
- [[MfaPronunciationProvider.RunG2pWithProgressAsync]]
- [[MfaWorkflow.RunChapterAsync]]
- [[PickupMfaRefinementService.RefineAsrTimingsAsync]]

