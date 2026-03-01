---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# AsrProcessor::AppendTokens
**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

## Summary
**Adds word-level ASR tokens from a segment into an output list after token aggregation/normalization.**

`AppendTokens` converts a segment’s raw Whisper tokens into normalized `AsrToken` words by calling `AggregateTokens` only when `segment.Tokens` is non-null/non-empty. If aggregation returns a non-empty list, it appends each aggregated token into the caller-provided `tokens` list in order. When no raw tokens exist or aggregation yields none, the method is a no-op.


#### [[AsrProcessor.AppendTokens]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void AppendTokens(List<AsrToken> tokens, SegmentData segment)
```

**Calls ->**
- [[AsrProcessor.AggregateTokens_2]]

**Called-by <-**
- [[AsrProcessor.RunWhisperPassAsync]]

