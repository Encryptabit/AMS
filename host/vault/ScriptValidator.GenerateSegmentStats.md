---
namespace: "Ams.Core.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# ScriptValidator::GenerateSegmentStats
**Path**: `Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs`

## Summary
**Creates a single transcript-level segment metric entry containing normalized expected/actual text, inferred timing bounds, and word error rate for validation reporting.**

`GenerateSegmentStats` builds and returns a `List<SegmentStats>`, immediately returning empty when `asrResponse.HasWords` is false. It derives one overall time window for the transcript from token timings when present (`first token start` to `last token start + duration`), otherwise from segment bounds (`first.StartSec` to `last.EndSec`). It normalizes `scriptText` and the joined ASR words with `TextNormalizer.Normalize(..., _options.ExpandContractions, _options.RemoveNumbers)`, computes WER via `CalculateSegmentWER`, and appends exactly one `SegmentStats` record (`Index = 0`, `Confidence = 0.0`).


#### [[ScriptValidator.GenerateSegmentStats]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private List<SegmentStats> GenerateSegmentStats(AsrResponse asrResponse, string scriptText)
```

**Calls ->**
- [[TextNormalizer.Normalize]]
- [[ScriptValidator.CalculateSegmentWER]]

**Called-by <-**
- [[ScriptValidator.Validate]]

