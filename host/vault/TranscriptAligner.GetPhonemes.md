---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
---
# TranscriptAligner::GetPhonemes
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

## Summary
**Fetches phoneme variants for a token index with null/bounds/empty guards.**

GetPhonemes safely retrieves an optional phoneme-variant array at a given token index. It returns `null` when the source list is null, the index is out of bounds, or the retrieved entry is null/empty. Otherwise it returns the existing `string[]` entry unchanged.


#### [[TranscriptAligner.GetPhonemes]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string[] GetPhonemes(IReadOnlyList<string[]> list, int index)
```

**Called-by <-**
- [[TranscriptAligner.AlignWindows]]

