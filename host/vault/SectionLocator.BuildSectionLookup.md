---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 4
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# SectionLocator::BuildSectionLookup
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`

## Summary
**Builds numeric and normalized-title candidate maps from book sections for fast section-title resolution.**

BuildSectionLookup precomputes two lookup indices over section metadata to support title/number resolution. For each `SectionRange`, it normalizes title text (`NormalizeTokens`), collapses numeric phrases (`CollapseNumberTokens`), generates match variants (`BuildNormalizedVariants`), and creates a `SectionCandidate` with `NormalizedOriginal = string.Join(" ", collapsed)`. It inserts candidates into `byNumber` when `ExtractLeadingNumber` succeeds and into `byNormalized` for every generated variant, appending to list buckets to preserve ambiguity sets. The method returns a `SectionLookup` containing both dictionaries for downstream disambiguation.


#### [[SectionLocator.BuildSectionLookup]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static SectionLocator.SectionLookup BuildSectionLookup(IReadOnlyList<SectionRange> sections)
```

**Calls ->**
- [[SectionLocator.BuildNormalizedVariants]]
- [[SectionLocator.CollapseNumberTokens]]
- [[SectionLocator.ExtractLeadingNumber]]
- [[SectionLocator.NormalizeTokens]]

**Called-by <-**
- [[SectionLocator.ResolveSectionByTitle]]

