---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 1
tags:
  - method
---
# TranscriptAligner::HasExactPhonemeMatch
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`


#### [[TranscriptAligner.HasExactPhonemeMatch]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool HasExactPhonemeMatch(string[] bookPhonemes, string[] asrPhonemes)
```

**Calls ->**
- [[PhonemeComparer.Equals]]

**Called-by <-**
- [[TranscriptAligner.SubCost]]

