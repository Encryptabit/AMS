---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 4
tags:
  - method
---
# SectionLocator::BuildSectionLookup
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`


#### [[SectionLocator.BuildSectionLookup]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static SectionLocator.SectionLookup BuildSectionLookup(IReadOnlyList<SectionRange> sections)
```

**Calls ->**
- [[SectionLocator.BuildNormalizedVariants]]
- [[SectionLocator.CollapseNumberTokens]]
- [[SectionLocator.ExtractLeadingNumber]]
- [[SectionLocator.NormalizeTokens]]

**Called-by <-**
- [[SectionLocator.ResolveSectionByTitle]]

