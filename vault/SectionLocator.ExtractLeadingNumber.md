---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "private"
complexity: 12
fan_in: 2
fan_out: 2
tags:
  - method
---
# SectionLocator::ExtractLeadingNumber
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`


#### [[SectionLocator.ExtractLeadingNumber]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static int? ExtractLeadingNumber(IReadOnlyList<string> tokens)
```

**Calls ->**
- [[SectionLocator.TryParseEmbeddedChapterNumber]]
- [[SectionLocator.TryParseFullNumber]]

**Called-by <-**
- [[SectionLocator.BuildSectionLookup]]
- [[SectionLocator.ResolveSectionByTitle]]

