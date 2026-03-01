---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "private"
complexity: 6
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# SectionLocator::NormalizeTokens
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`

## Summary
**Converts free-form text into a normalized list of lowercase alphanumeric tokens split on non-alphanumeric boundaries.**

NormalizeTokens tokenizes arbitrary text into lowercase alphanumeric runs by scanning characters and accumulating `char.IsLetterOrDigit` values in a `StringBuilder`. On any non-alphanumeric delimiter, it flushes the current buffer to the output list and clears it, then performs a final flush after the loop. It returns an empty list for null/whitespace input and never emits empty tokens.


#### [[SectionLocator.NormalizeTokens]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<string> NormalizeTokens(string text)
```

**Called-by <-**
- [[SectionLocator.BuildSectionLookup]]
- [[SectionLocator.ResolveSectionByTitle]]

