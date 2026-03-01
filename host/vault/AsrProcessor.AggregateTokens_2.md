---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
---
# AsrProcessor::AggregateTokens
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Bridges raw Whisper tokens into the common token-aggregation routine using Whisper-specific time/text extraction.**

This overload is a thin adapter that forwards `WhisperToken[]` to the generic `AggregateTokens<TToken>` pipeline with Whisper-specific selectors: start/end converted from centiseconds (`/ 100.0`) and text from `token.Text`. It normalizes token timing by ensuring end is not before start (`Math.Max(start, end)`) via the passed end selector and returns the aggregated `List<AsrToken>` from the shared implementation.


#### [[AsrProcessor.AggregateTokens_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<AsrToken> AggregateTokens(WhisperToken[] rawTokens)
```

**Calls ->**
- [[AsrProcessor.AggregateTokens]]

**Called-by <-**
- [[AsrProcessor.AppendTokens]]

