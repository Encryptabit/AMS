---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "private"
complexity: 5
fan_in: 4
fan_out: 0
tags:
  - method
---
# SectionLocator::TryParseIntToken
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`


#### [[SectionLocator.TryParseIntToken]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool TryParseIntToken(string token, out int value)
```

**Called-by <-**
- [[SectionLocator.BuildNormalizedVariants]]
- [[SectionLocator.TryParseCombinedNumber]]
- [[SectionLocator.TryParseEmbeddedChapterNumber]]
- [[SectionLocator.TryParseFullNumber]]

