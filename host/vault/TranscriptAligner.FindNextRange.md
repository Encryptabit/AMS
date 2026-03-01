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
# TranscriptAligner::FindNextRange
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

## Summary
**Locates the closest subsequent sentence with a concrete script range and returns its bounds.**

`FindNextRange` walks forward from the provided `index` through `sentences`, calling `TryGetConcreteRange` on each element. It returns the first concrete `(start, end)` script range encountered, enabling nearest-right boundary lookup. If none is found before the end of the list, it returns `null`.


#### [[TranscriptAligner.FindNextRange]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (int Start, int End)? FindNextRange(IReadOnlyList<SentenceAlign> sentences, int index)
```

**Calls ->**
- [[TranscriptAligner.TryGetConcreteRange]]

**Called-by <-**
- [[TranscriptAligner.SynthesizeMissingScriptRanges]]

