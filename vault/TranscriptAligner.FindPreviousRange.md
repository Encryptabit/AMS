---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# TranscriptAligner::FindPreviousRange
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`


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

