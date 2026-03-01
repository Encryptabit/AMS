---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/validation
---
# SectionLocator::BuildNormalizedVariants
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`

## Summary
**Produces alternative normalized chapter-label strings (with optional keyword/number trimming) for robust section matching.**

BuildNormalizedVariants generates multiple canonical string forms from tokenized chapter text to improve lookup hit rate. It adds the full token sequence, then a keyword-trimmed sequence from `TrimLeadingKeywords`, each via a local `AddVariant` helper that joins tokens with spaces and ignores empty results. If the trimmed variant starts with an integer token (`TryParseIntToken`), it also adds a title-only variant with that leading number removed. Results are deduplicated in an ordinal `HashSet<string>`.


#### [[SectionLocator.BuildNormalizedVariants]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static HashSet<string> BuildNormalizedVariants(IReadOnlyList<string> tokens)
```

**Calls ->**
- [[AddVariant]]
- [[SectionLocator.TrimLeadingKeywords]]
- [[SectionLocator.TryParseIntToken]]

**Called-by <-**
- [[SectionLocator.BuildSectionLookup]]
- [[SectionLocator.ResolveSectionByTitle]]

