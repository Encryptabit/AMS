---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 3
tags:
  - method
---
# SectionLocator::BuildNormalizedVariants
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`


#### [[SectionLocator.BuildNormalizedVariants]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static HashSet<string> BuildNormalizedVariants(IReadOnlyList<string> tokens)
```

**Calls ->**
- [[AddVariant]]
- [[SectionLocator.TrimLeadingKeywords]]
- [[SectionLocator.TryParseIntToken]]

**Called-by <-**
- [[SectionLocator.BuildSectionLookup]]
- [[SectionLocator.ResolveSectionByTitle]]

