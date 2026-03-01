---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AsrProcessor::HasExplicitWordBoundary
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Detects whether a token carries an explicit leading boundary that should trigger a new aggregated word.**

`HasExplicitWordBoundary` checks whether a token explicitly begins a new word by inspecting its first character. It returns `false` for null/empty input; otherwise it returns `true` when the first character is whitespace or one of the tokenizer boundary markers `▁`, `Ġ`, or `Ċ`. This gives aggregation logic a deterministic boundary signal independent of token content length.


#### [[AsrProcessor.HasExplicitWordBoundary]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool HasExplicitWordBoundary(string text)
```

**Called-by <-**
- [[AsrProcessor.AggregateTokens]]

