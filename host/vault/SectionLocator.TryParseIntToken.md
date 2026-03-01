---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "private"
complexity: 5
fan_in: 4
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# SectionLocator::TryParseIntToken
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`

## Summary
**Recognizes numeric tokens in plain or suffixed-ordinal form and outputs the parsed integer value.**

TryParseIntToken parses a token as an integer, first via direct `int.TryParse(token, out value)`. If that fails and the token length exceeds 2, it checks for ordinal suffixes (`st`, `nd`, `rd`, `th`), slices off the suffix, and retries parsing the numeric prefix with span-based `int.TryParse`. On success it returns `true`; otherwise it sets `value = 0` and returns `false`.


#### [[SectionLocator.TryParseIntToken]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool TryParseIntToken(string token, out int value)
```

**Called-by <-**
- [[SectionLocator.BuildNormalizedVariants]]
- [[SectionLocator.TryParseCombinedNumber]]
- [[SectionLocator.TryParseEmbeddedChapterNumber]]
- [[SectionLocator.TryParseFullNumber]]

