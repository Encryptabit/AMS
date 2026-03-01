---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "private"
complexity: 5
fan_in: 2
fan_out: 0
tags:
  - method
---
# SectionLocator::TryParseRoman
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`


#### [[SectionLocator.TryParseRoman]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool TryParseRoman(string token, out int value)
```

**Called-by <-**
- [[SectionLocator.TryParseCombinedNumber]]
- [[SectionLocator.TryParseFullNumber]]

