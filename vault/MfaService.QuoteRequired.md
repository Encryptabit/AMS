---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaService.cs"
access_modifier: "private"
complexity: 2
fan_in: 4
fan_out: 1
tags:
  - method
---
# MfaService::QuoteRequired
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaService.cs`


#### [[MfaService.QuoteRequired]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string QuoteRequired(string value)
```

**Calls ->**
- [[MfaService.Quote]]

**Called-by <-**
- [[MfaService.AddWordsAsync]]
- [[MfaService.AlignAsync]]
- [[MfaService.GeneratePronunciationsAsync]]
- [[MfaService.ValidateAsync]]

