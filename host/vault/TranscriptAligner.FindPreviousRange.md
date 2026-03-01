---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
---
# TranscriptAligner::FindPreviousRange
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

## Summary
**Finds the nearest preceding sentence that has a concrete script range and returns its bounds.**

`FindPreviousRange` performs a backward linear scan from `index` to `0` over `sentences`, using `TryGetConcreteRange` to detect the first sentence with a concrete `ScriptRange`. It returns that range as a value tuple `(start, end)` immediately when found. If no prior concrete range exists, it returns `null`.


#### [[TranscriptAligner.FindPreviousRange]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (int Start, int End)? FindPreviousRange(IReadOnlyList<SentenceAlign> sentences, int index)
```

**Calls ->**
- [[TranscriptAligner.TryGetConcreteRange]]

**Called-by <-**
- [[TranscriptAligner.SynthesizeMissingScriptRanges]]

