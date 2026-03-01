---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaService.cs"
access_modifier: "public"
complexity: 5
fan_in: 1
fan_out: 3
tags:
  - method
---
# MfaService::ValidateAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaService.cs`


#### [[MfaService.ValidateAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<MfaCommandResult> ValidateAsync(MfaChapterContext context, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[MfaService.Quote]]
- [[MfaService.QuoteRequired]]
- [[MfaService.RunCommandAsync]]

**Called-by <-**
- [[PickupMfaRefinementService.RefineAsrTimingsAsync]]

