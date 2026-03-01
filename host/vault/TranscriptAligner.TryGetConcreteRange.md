---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 2
fan_in: 3
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptAligner::TryGetConcreteRange
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

## Summary
**Determines whether a sentence has a concrete script range and, if so, returns its start/end indices.**

`TryGetConcreteRange` is a small extractor that pattern-matches `sentence.ScriptRange` for non-null `Start` and `End` integers (`{ Start: int s, End: int e }`). On success it assigns `out` parameters to those values and returns `true`; otherwise it writes `default` to both outs and returns `false`. This gives callers a single null-safe check for whether a sentence has a fully materialized script range.


#### [[TranscriptAligner.TryGetConcreteRange]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool TryGetConcreteRange(SentenceAlign sentence, out int start, out int end)
```

**Called-by <-**
- [[TranscriptAligner.FindNextRange]]
- [[TranscriptAligner.FindPreviousRange]]
- [[TranscriptAligner.SynthesizeMissingScriptRanges]]

