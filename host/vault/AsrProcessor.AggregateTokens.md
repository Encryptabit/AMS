---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 4
tags:
  - method
  - llm/utility
  - llm/validation
---
# AsrProcessor::AggregateTokens
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Aggregates model token fragments into normalized, timed word tokens using boundary-aware merging rules.**

`AggregateTokens<TToken>` streams raw token pieces into word-level `AsrToken` objects by filtering unusable inputs (`null/empty` text, special bracketed tokens, whitespace-only normalized text) and concatenating normalized fragments in a `StringBuilder`. It uses `HasExplicitWordBoundary` to decide when to flush the current word before appending a new boundary-starting token, tracks `wordStart` from the first fragment, and extends `wordEnd` with each token’s bounded end time (`Math.Max(start, endSelector(token))`). A local `Flush` function emits `new AsrToken(wordStart, Math.Max(0.05, wordEnd - wordStart), builder.ToString())`, then resets state; flush runs on boundary transitions and once at end. The result is an ordered list of merged tokens with normalized text and minimum duration guarantees.


#### [[AsrProcessor.AggregateTokens]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<AsrToken> AggregateTokens<TToken>(IEnumerable<TToken> rawTokens, Func<TToken, double> startSelector, Func<TToken, double> endSelector, Func<TToken, string> textSelector)
```

**Calls ->**
- [[Flush]]
- [[AsrProcessor.HasExplicitWordBoundary]]
- [[AsrProcessor.IsSpecialToken]]
- [[AsrProcessor.NormalizeTokenText]]

**Called-by <-**
- [[AsrProcessor.AggregateTokens_2]]

