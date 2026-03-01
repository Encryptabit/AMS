---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
---
# SectionLocator::TryParseEmbeddedChapterNumber
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`


#### [[SectionLocator.TryParseEmbeddedChapterNumber]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool TryParseEmbeddedChapterNumber(string token, out int value)
```

**Calls ->**
- [[SectionLocator.TryParseIntToken]]

**Called-by <-**
- [[SectionLocator.ExtractLeadingNumber]]

