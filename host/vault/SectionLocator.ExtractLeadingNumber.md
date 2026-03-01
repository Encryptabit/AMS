---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "private"
complexity: 12
fan_in: 2
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# SectionLocator::ExtractLeadingNumber
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`

## Summary
**Extracts the most likely chapter number from a token list using keyword-aware and filename-oriented numeric heuristics.**

ExtractLeadingNumber heuristically derives a chapter number from normalized tokens, returning `null` when none is found. It handles filename patterns with dual leading numerics by preferring the second token as the starting scan point when both token 0 and 1 parse as full numbers. It then scans forward: if a chapter keyword (`chapter`/`ch`) appears, it tries to parse the next token as a full number; otherwise it attempts embedded forms like `chapter12` via `TryParseEmbeddedChapterNumber`. If no in-scan match is found, it falls back to parsing token 0 as a full number.


#### [[SectionLocator.ExtractLeadingNumber]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static int? ExtractLeadingNumber(IReadOnlyList<string> tokens)
```

**Calls ->**
- [[SectionLocator.TryParseEmbeddedChapterNumber]]
- [[SectionLocator.TryParseFullNumber]]

**Called-by <-**
- [[SectionLocator.BuildSectionLookup]]
- [[SectionLocator.ResolveSectionByTitle]]

