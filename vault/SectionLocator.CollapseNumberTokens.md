---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 1
tags:
  - method
---
# SectionLocator::CollapseNumberTokens
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`


#### [[SectionLocator.CollapseNumberTokens]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<string> CollapseNumberTokens(IReadOnlyList<string> tokens)
```

**Calls ->**
- [[SectionLocator.TryParseCombinedNumber]]

**Called-by <-**
- [[SectionLocator.BuildSectionLookup]]
- [[SectionLocator.ResolveSectionByTitle]]

