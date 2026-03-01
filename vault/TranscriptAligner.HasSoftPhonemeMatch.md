---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "private"
complexity: 11
fan_in: 1
fan_out: 2
tags:
  - method
---
# TranscriptAligner::HasSoftPhonemeMatch
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`


#### [[TranscriptAligner.HasSoftPhonemeMatch]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool HasSoftPhonemeMatch(string[] bookPhonemes, string[] asrPhonemes, double threshold)
```

**Calls ->**
- [[PhonemeComparer.Similarity]]
- [[PhonemeComparer.Tokenize]]

**Called-by <-**
- [[TranscriptAligner.SubCost]]

