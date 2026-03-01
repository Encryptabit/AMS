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
# SectionLocator::TryParseFullNumber
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`

## Summary
**Parses one or two tokens into an integer using multiple chapter-number representations.**

TryParseFullNumber parses a numeric expression beginning at `tokens[index]` and reports both `value` and token `consumed` count (default `1`). It rejects out-of-range indices, then attempts formats in order: integer token (`TryParseIntToken`), Roman numeral (`TryParseRoman`), spelled ordinals, spelled units, and spelled tens. For tens tokens, it optionally consumes the next unit token (`unit < 10`) to form composite values like "thirty four", updating `consumed` to `2`. It returns `false` when no supported format matches.


#### [[SectionLocator.TryParseFullNumber]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool TryParseFullNumber(IReadOnlyList<string> tokens, int index, out int value, out int consumed)
```

**Calls ->**
- [[SectionLocator.TryParseIntToken]]
- [[SectionLocator.TryParseRoman]]

**Called-by <-**
- [[SectionLocator.ExtractLeadingNumber]]

