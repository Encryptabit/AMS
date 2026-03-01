---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaService.cs"
access_modifier: "private"
complexity: 2
fan_in: 4
fan_out: 0
tags:
  - method
---
# MfaService::Quote
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaService.cs`


#### [[MfaService.Quote]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string Quote(string value)
```

**Called-by <-**
- [[MfaService.AddWordsAsync]]
- [[MfaService.GeneratePronunciationsAsync]]
- [[MfaService.QuoteRequired]]
- [[MfaService.ValidateAsync]]

