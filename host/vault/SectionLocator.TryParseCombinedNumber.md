---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "private"
complexity: 10
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# SectionLocator::TryParseCombinedNumber
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`

## Summary
**Parses one or two tokens as a canonical integer using multiple numeric formats and reports token consumption.**

TryParseCombinedNumber attempts to parse a numeric value starting at `tokens[index]`, returning both the parsed `value` and how many tokens were consumed. It initializes outputs to zero, bounds-checks `index`, then tries parsers in order: integer token (`TryParseIntToken`), spelled ordinal (`SpelledOrdinals`), Roman numeral (`TryParseRoman`), spelled unit (`SpelledUnits`), and tens words (`SpelledTens`). For tens, it optionally consumes a following unit token (`unit < 10`) to form values like "twenty one", setting `consumed = 2`; otherwise consumed stays `1`. It returns `false` when no supported numeric form matches.


#### [[SectionLocator.TryParseCombinedNumber]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool TryParseCombinedNumber(IReadOnlyList<string> tokens, int index, out int value, out int consumed)
```

**Calls ->**
- [[SectionLocator.TryParseIntToken]]
- [[SectionLocator.TryParseRoman]]

**Called-by <-**
- [[SectionLocator.CollapseNumberTokens]]

