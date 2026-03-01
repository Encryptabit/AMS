---
namespace: "Ams.Core.Prosody"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# PauseDynamicsService::BuildHeadingParagraphSet
**Path**: `Projects/AMS/host/Ams.Core/Prosody/PauseDynamicsService.cs`

## Summary
**Builds a set of paragraph IDs classified as headings from book index metadata.**

`BuildHeadingParagraphSet` scans `bookIndex.Paragraphs` and collects paragraph indices whose `Kind` equals `"Heading"` using case-insensitive ordinal comparison. It skips null paragraph entries and returns the resulting `HashSet<int>` of heading paragraph IDs. The method is a pure extractor with no fallback heuristics beyond the `Kind` string match.


#### [[PauseDynamicsService.BuildHeadingParagraphSet]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static HashSet<int> BuildHeadingParagraphSet(BookIndex bookIndex)
```

**Called-by <-**
- [[PauseDynamicsService.AnalyzeChapter]]

