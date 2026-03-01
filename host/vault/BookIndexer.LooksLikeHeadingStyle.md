---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 16
fan_in: 2
fan_out: 1
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/validation
---
# BookIndexer::LooksLikeHeadingStyle
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

> [!danger] High Complexity (16)
> Cyclomatic complexity: 16. Consider refactoring into smaller methods.

## Summary
**Determines whether paragraph style metadata suggests a structural heading suitable for section detection.**

`LooksLikeHeadingStyle` returns a heuristic heading signal from optional `style`/`kind` metadata. It short-circuits `true` when `kind == "Heading"` (case-insensitive), returns `false` for blank styles, and rejects known non-section styles via `IsNonSectionParagraphStyle(style)`. Otherwise it checks style text for a curated keyword set (`heading`, `title`, `chapter`, `section`, `part`, `book`, `prologue`, `epilogue`, `foreword`, `afterword`, `preface`, `acknowledg`) using case-insensitive `Contains`.


#### [[BookIndexer.LooksLikeHeadingStyle]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool LooksLikeHeadingStyle(string style, string kind)
```

**Calls ->**
- [[BookIndexer.IsNonSectionParagraphStyle]]

**Called-by <-**
- [[BookIndexer.Process]]
- [[BookIndexer.ShouldStartSection]]

