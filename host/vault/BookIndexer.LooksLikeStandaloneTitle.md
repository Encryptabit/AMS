---
namespace: "Ams.Core.Runtime.Documents"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs"
access_modifier: "private"
complexity: 15
fan_in: 1
fan_out: 1
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/validation
---
# BookIndexer::LooksLikeStandaloneTitle
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookIndexer.cs`

> [!danger] High Complexity (15)
> Cyclomatic complexity: 15. Consider refactoring into smaller methods.

## Summary
**Determines whether a text line resembles a standalone section title suitable for section-start detection.**

`LooksLikeStandaloneTitle` heuristically classifies short heading-like lines while excluding TOC artifacts and sentence-like text. It rejects null/whitespace, overly long text (`>120`), TOC-style entries (`LooksLikeTableOfContentsEntry`), and strings containing `?`, `!`, or `;`; it then accepts explicit numbered-heading patterns (`NumberedHeadingRegex`). Failing that, it computes uppercase letter ratio (accepts `>= 0.6`) and finally checks title-cased word patterns (1–8 words, each starting uppercase). This staged logic favors likely chapter/section titles without relying on a single regex.


#### [[BookIndexer.LooksLikeStandaloneTitle]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool LooksLikeStandaloneTitle(string text)
```

**Calls ->**
- [[BookIndexer.LooksLikeTableOfContentsEntry]]

**Called-by <-**
- [[BookIndexer.ShouldStartSection]]

