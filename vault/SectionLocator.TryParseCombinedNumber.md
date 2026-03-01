---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "private"
complexity: 10
fan_in: 1
fan_out: 2
tags:
  - method
---
# SectionLocator::TryParseCombinedNumber
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`


#### [[SectionLocator.TryParseCombinedNumber]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool TryParseCombinedNumber(IReadOnlyList<string> tokens, int index, out int value, out int consumed)
```

**Calls ->**
- [[SectionLocator.TryParseIntToken]]
- [[SectionLocator.TryParseRoman]]

**Called-by <-**
- [[SectionLocator.CollapseNumberTokens]]

