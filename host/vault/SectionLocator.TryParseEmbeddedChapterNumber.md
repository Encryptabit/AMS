---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# SectionLocator::TryParseEmbeddedChapterNumber
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`

## Summary
**Parses numbers embedded immediately after chapter keywords in a single token.**

TryParseEmbeddedChapterNumber detects compact chapter labels where a known chapter keyword is directly prefixed to a numeric suffix (e.g., `chapter12`, `ch7`). It iterates `LeadingChapterKeywords`, checks `token.StartsWith(keyword, StringComparison.Ordinal)` and ensures extra characters exist, then parses the suffix with `TryParseIntToken`. On first successful parse it returns `true` with the parsed `value`; otherwise it sets `value = 0` and returns `false`.


#### [[SectionLocator.TryParseEmbeddedChapterNumber]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool TryParseEmbeddedChapterNumber(string token, out int value)
```

**Calls ->**
- [[SectionLocator.TryParseIntToken]]

**Called-by <-**
- [[SectionLocator.ExtractLeadingNumber]]

