---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "public"
complexity: 6
fan_in: 2
fan_out: 4
tags:
  - method
---
# TranscriptAligner::SubCost
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`


#### [[TranscriptAligner.SubCost]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static double SubCost(string bookTok, string asrTok, IReadOnlyDictionary<string, string> equiv, string[] bookPhonemes = null, string[] asrPhonemes = null, double phonemeSoftThreshold = 1.01)
```

**Calls ->**
- [[TranscriptAligner.Equivalent]]
- [[TranscriptAligner.HasExactPhonemeMatch]]
- [[TranscriptAligner.HasSoftPhonemeMatch]]
- [[TranscriptAligner.LevLe1]]

**Called-by <-**
- [[TranscriptAligner.AlignWindows]]
- [[TxAlignTests.SubCost_DefaultDisablesSoftPhonemeMatching]]

