---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 8
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookIndexer::GetHeadingLevel
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Derives a heading hierarchy level from a style label such as “Heading 1”/“Heading1”.**

`GetHeadingLevel` extracts a numeric heading depth from a style string using lightweight parsing rather than regex. It returns `0` for null/empty styles or when `"Heading"` is not present (case-insensitive), then scans trailing characters after the keyword to find the first digit run and parses it with `int.TryParse`. If digits are present but parsing fails, it breaks and falls back; if `"Heading"` exists without explicit digits, it returns `1` as the default level.


#### [[BookIndexer.GetHeadingLevel]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static int GetHeadingLevel(string style)
```

**Called-by <-**
- [[BookIndexer.Process]]

