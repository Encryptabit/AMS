---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaService.cs"
access_modifier: "private"
complexity: 2
fan_in: 4
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# MfaService::QuoteRequired
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaService.cs`

## Summary
**It validates that a required CLI argument is present and returns it as a safely quoted string.**

`QuoteRequired` enforces that `value` is non-null/non-whitespace, throwing `ArgumentException("Required value was null or whitespace", nameof(value))` when invalid. For valid input it trims surrounding whitespace and delegates to `Quote(...)` to produce the escaped, double-quoted token.


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

