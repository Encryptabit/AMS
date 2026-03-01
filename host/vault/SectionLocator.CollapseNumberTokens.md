---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# SectionLocator::CollapseNumberTokens
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`

## Summary
**Normalizes tokenized number expressions into single canonical numeric tokens within a copied token list.**

CollapseNumberTokens rewrites a token stream by greedily collapsing parseable numeric phrases into canonical numeric string tokens. It iterates with an index cursor, calling `TryParseCombinedNumber(tokens, i, out value, out consumed)`; on success it appends `value.ToString()` and advances by `consumed`, otherwise it copies the original token and advances by one. The method preserves token order while normalizing heterogeneous number expressions (digits/words/roman, as supported by the parser).


#### [[SectionLocator.CollapseNumberTokens]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<string> CollapseNumberTokens(IReadOnlyList<string> tokens)
```

**Calls ->**
- [[SectionLocator.TryParseCombinedNumber]]

**Called-by <-**
- [[SectionLocator.BuildSectionLookup]]
- [[SectionLocator.ResolveSectionByTitle]]

