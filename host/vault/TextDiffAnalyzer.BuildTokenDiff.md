---
namespace: "Ams.Core.Processors.Diffing"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# TextDiffAnalyzer::BuildTokenDiff
**Path**: `Projects/AMS/host/Ams.Core/Processors/Diffing/TextDiffAnalyzer.cs`

## Summary
**Generates a compact, dictionary-backed token diff between reference and hypothesis token sequences.**

`BuildTokenDiff` computes a token-level diff by first short-circuiting to an empty `TokenDiffResult` when both inputs are empty. It builds a shared token dictionary/map and locally `Encode`s each token sequence into a compact char string where each unique token is assigned an index; if unique-token count reaches `char.MaxValue`, it throws `InvalidOperationException`. It then runs `diff_match_patch.diff_main` on the encoded strings, applies semantic cleanup, and returns the resulting `Diff` list together with the dictionary needed for later token decoding.


#### [[TextDiffAnalyzer.BuildTokenDiff]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static TextDiffAnalyzer.TokenDiffResult BuildTokenDiff(IReadOnlyList<string> referenceTokens, IReadOnlyList<string> hypothesisTokens)
```

**Calls ->**
- [[Encode]]

**Called-by <-**
- [[TextDiffAnalyzer.Analyze_2]]

