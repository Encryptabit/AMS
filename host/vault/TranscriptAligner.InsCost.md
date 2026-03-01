---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "public"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptAligner::InsCost
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

## Summary
**Computes insertion cost, discounting ASR filler tokens relative to normal tokens.**

InsCost applies a lower insertion penalty for filler tokens during DP alignment. It checks `fillers.Contains(asrTok)` and returns `0.3` for filler words, otherwise `1.0`.


#### [[TranscriptAligner.InsCost]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static double InsCost(string asrTok, ISet<string> fillers)
```

**Called-by <-**
- [[TranscriptAligner.AlignWindows]]

