---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "public"
complexity: 7
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ChapterFileComparer::Compare
**Path**: `Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs`

## Summary
**It orders chapter `FileInfo` instances for REPL state operations using derived chapter sort keys and comparer-contract return values.**

`ReplState.ChapterFileComparer.Compare(FileInfo x, FileInfo y)` implements comparer semantics by computing each file’s chapter sort token through `GetSortKey`, comparing those keys, and returning the signed integer required by `IComparer<FileInfo>`. Its stated complexity (7) implies multiple branch paths, typically for null/reference-equality guards and tie-break behavior when keys are equal, so ordering remains deterministic.


#### [[Ams.Cli.Repl.ReplState.ChapterFileComparer.Compare]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public int Compare(FileInfo x, FileInfo y)
```

**Calls ->**
- [[Ams.Cli.Repl.ReplState.ChapterFileComparer.GetSortKey]]

