---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 7
fan_in: 2
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptAligner::BuildNormalizedWordString
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

## Summary
**Constructs a normalized character sequence from an ASR word span for downstream alignment quality calculations.**

`BuildNormalizedWordString(AsrResponse, int?, int?)` produces a canonical ASR substring by iterating a validated/clamped word-index window and normalizing each token. It returns `string.Empty` when `asr` is null, bounds are missing, or `asr.HasWords` is false; otherwise it clamps `start`/`end` into `[0, WordCount-1]` with `end >= start`. For each index it reads `asr.GetWord(i)`, skips null/empty words, and appends normalized characters through `AppendNormalized` (alphanumeric lowercase only).


#### [[TranscriptAligner.BuildNormalizedWordString]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildNormalizedWordString(AsrResponse asr, int? start, int? end)
```

**Calls ->**
- [[AsrResponse.GetWord]]
- [[TranscriptAligner.AppendNormalized]]

**Called-by <-**
- [[TranscriptAligner.ComputeCer]]
- [[TranscriptAligner.Rollup_2]]

