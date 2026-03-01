---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# TranscriptAligner::DelCost
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`

## Summary
**Provides a constant cost for deleting a book token during alignment.**

DelCost is a fixed deletion-penalty function for the aligner. It ignores the token content and always returns `1.0`.


#### [[TranscriptAligner.DelCost]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static double DelCost(string bookTok)
```

**Called-by <-**
- [[TranscriptAligner.AlignWindows]]

