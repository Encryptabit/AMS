---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# SectionLocator::TrimLeadingKeywords
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`

## Summary
**Strips leading chapter-keyword tokens (e.g., `chapter`, `ch`) from a tokenized label.**

TrimLeadingKeywords removes a contiguous prefix of heading markers from the token list using `LeadingChapterKeywords` membership checks. It increments an index while leading tokens match, then returns the remaining suffix via `tokens.Skip(idx).ToList()`. If no leading keyword is present, it returns a full copy of the original sequence.


#### [[SectionLocator.TrimLeadingKeywords]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<string> TrimLeadingKeywords(IReadOnlyList<string> tokens)
```

**Called-by <-**
- [[SectionLocator.BuildNormalizedVariants]]

