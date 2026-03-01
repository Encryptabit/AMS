---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 11
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookIndexer::ClassifySectionKind
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

## Summary
**Classifies a section heading into a canonical section-kind label based on keyword matching.**

`ClassifySectionKind` maps heading text to a normalized section kind using lowercase substring heuristics. It returns `"chapter"` for null/whitespace input, then checks ordered keyword matches (`prologue`, `epilogue`, `prelude`, `foreword`, `introduction`, `afterword`, `acknowledg`, `appendix`, `chapter`) and returns the first matching canonical label (including `"acknowledgments"` for partial “acknowledg”). If no keyword matches, it defaults to `"chapter"`.


#### [[BookIndexer.ClassifySectionKind]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ClassifySectionKind(string headingText)
```

**Called-by <-**
- [[BookIndexer.Process]]

