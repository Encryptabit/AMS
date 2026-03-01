---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# ParagraphCollector::AbsorbSentenceDurations
**Path**: `home/cari/repos/AMS/host/Ams.Core/Prosody/PauseMapBuilder.cs`


#### [[ParagraphCollector.AbsorbSentenceDurations]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void AbsorbSentenceDurations(IEnumerable<PauseMapBuilder.SentenceCollector> sentenceCollectors)
```

**Calls ->**
- [[ParagraphCollector.AddDurationRange]]

**Called-by <-**
- [[PauseMapBuilder.Build]]

