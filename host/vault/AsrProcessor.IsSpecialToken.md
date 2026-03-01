---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# AsrProcessor::IsSpecialToken
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Identifies bracket-delimited special tokens that should be ignored during token aggregation.**

`IsSpecialToken` is a simple lexical filter that returns `true` only when a token string starts with `'['` and ends with `']'`. It treats bracket-wrapped markers as non-lexical/special tokens for upstream exclusion logic.


#### [[AsrProcessor.IsSpecialToken]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsSpecialToken(string text)
```

**Called-by <-**
- [[AsrProcessor.AggregateTokens]]

