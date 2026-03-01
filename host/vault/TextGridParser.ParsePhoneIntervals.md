---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/TextGridParser.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# TextGridParser::ParsePhoneIntervals
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/TextGridParser.cs`

## Summary
**Parses intervals from phone-like TextGrid tiers (`phones`, `phonemes`, or `segments`).**

ParsePhoneIntervals delegates to `ParseIntervals` with a tier predicate that matches phone-tier names case-insensitively. It accepts `"phones"`, `"phonemes"`, or `"segments"` as valid tier labels, allowing compatibility across MFA/TextGrid naming variants. The underlying file existence checks and interval extraction are performed by `ParseIntervals`.


#### [[TextGridParser.ParsePhoneIntervals]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyList<TextGridInterval> ParsePhoneIntervals(string textGridPath)
```

**Calls ->**
- [[TextGridParser.ParseIntervals]]

