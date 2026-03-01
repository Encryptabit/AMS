---
namespace: "Ams.Core.Processors.Alignment.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# MfaTimingMerger::BuildTimedTgTokens
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Mfa/MfaTimingMerger.cs`

## Summary
**Transforms filtered TextGrid intervals into normalized, timestamped token entries for downstream alignment.**

BuildTimedTgTokens converts TextGrid word intervals into a sequenced list of timed alignment tokens. It iterates intervals, skipping blank text or non-positive durations (`!(it.End > it.Start)`), tokenizes each valid interval via `TokenizeForAlignment(..., forTextGrid: true)`, and emits `TgTok(seq++, tok, it.Start, it.End)` for each produced token. If tokenization yields nothing, it emits a fallback `UNK` token for that interval to preserve timing coverage. The output preserves interval order and assigns contiguous `TgSeq` ids.


#### [[MfaTimingMerger.BuildTimedTgTokens]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<TgTok> BuildTimedTgTokens(IEnumerable<TextGridWord> intervals)
```

**Calls ->**
- [[MfaTimingMerger.TokenizeForAlignment]]

**Called-by <-**
- [[MfaTimingMerger.MergeAndApply]]

