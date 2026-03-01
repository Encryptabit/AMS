---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# TextDiffAnalyzer::DecodeTokens
**Path**: `Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`

## Summary
**Decodes dictionary-indexed token characters back into their original token sequence for hydrated diff output.**

`DecodeTokens` reconstructs token strings from an encoded char sequence using the shared dictionary produced during encoding. It returns `Array.Empty<string>()` when the encoded payload is empty or the dictionary has no entries. Otherwise it iterates each character, treats the char code as a dictionary index, and appends the token only when the index is within `[0, dictionary.Count)`, silently skipping invalid codes. The output is a list sized to the encoded length upper bound.


#### [[TextDiffAnalyzer.DecodeTokens]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyList<string> DecodeTokens(string encoded, IReadOnlyList<string> dictionary)
```

**Called-by <-**
- [[TextDiffAnalyzer.Analyze_2]]

