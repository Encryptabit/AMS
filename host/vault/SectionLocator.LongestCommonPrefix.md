---
namespace: "Ams.Core.Processors.Alignment.Anchors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# SectionLocator::LongestCommonPrefix
**Path**: `Projects/AMS/host/Ams.Core/Processors/Alignment/Anchors/SectionLocator.cs`

## Summary
**Returns how many initial tokens two normalized token lists share before diverging.**

LongestCommonPrefix computes the count of matching leading tokens between two string sequences. It iterates up to `Math.Min(a.Count, b.Count)` and stops at the first ordinal-inequal pair using `string.Equals(..., StringComparison.Ordinal)`. The returned integer is the prefix length shared by both lists.


#### [[SectionLocator.LongestCommonPrefix]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static int LongestCommonPrefix(IReadOnlyList<string> a, IReadOnlyList<string> b)
```

**Called-by <-**
- [[SectionLocator.DetectSection]]

