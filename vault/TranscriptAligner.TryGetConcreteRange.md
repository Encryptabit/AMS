---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 2
fan_in: 3
fan_out: 0
tags:
  - method
---
# TranscriptAligner::TryGetConcreteRange
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`


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

