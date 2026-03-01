---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
---
# PauseMapBuilder::CreateParagraphCollectors
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`


#### [[PauseMapBuilder.CreateParagraphCollectors]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Dictionary<int, PauseMapBuilder.ParagraphCollector> CreateParagraphCollectors(IReadOnlyDictionary<int, IReadOnlyList<int>> paragraphSentenceOrder)
```

**Called-by <-**
- [[PauseMapBuilder.Build]]

