---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaService.cs"
access_modifier: "public"
complexity: 9
fan_in: 2
fan_out: 2
tags:
  - method
---
# MfaService::AlignAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaService.cs`


#### [[MfaService.AlignAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<MfaCommandResult> AlignAsync(MfaChapterContext context, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[MfaService.QuoteRequired]]
- [[MfaService.RunCommandAsync]]

**Called-by <-**
- [[MfaWorkflow.RunChapterAsync]]
- [[PickupMfaRefinementService.RefineAsrTimingsAsync]]

