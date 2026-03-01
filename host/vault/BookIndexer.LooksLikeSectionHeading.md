---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 3
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookIndexer::LooksLikeSectionHeading
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Determines whether text matches the configured section-heading pattern.**

`LooksLikeSectionHeading` is a lightweight predicate that validates input and delegates pattern recognition to `SectionTitleRegex`. It returns `false` for null/whitespace text, trims the input, then returns `true` when the compiled regex matches; otherwise `false`. The method centralizes regex-based heading detection behind a simple boolean API.


#### [[BookIndexer.LooksLikeSectionHeading]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool LooksLikeSectionHeading(string text)
```

**Called-by <-**
- [[BookIndexer.Process]]
- [[BookIndexer.ShouldStartSection]]

