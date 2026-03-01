---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# ChapterFileComparer::GetSortKey
**Path**: `Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs`

## Summary
**Generates a normalized `SortKey` from a `FileInfo` chapter filename so `Compare` can sort files by embedded number first and by name fallback.**

`GetSortKey` computes a deterministic sort tuple from a chapter file name by taking `Path.GetFileNameWithoutExtension(file.Name)`, matching the first numeric run with compiled `NumberRegex` (`\d+`), and parsing it through `int.TryParse`. On success it returns `new SortKey(0, primary, stem.ToLowerInvariant())`; on failure it returns `new SortKey(1, int.MaxValue, stem.ToLowerInvariant())`, which pushes non-numeric names behind numeric ones while keeping a lowercase lexical tiebreak component.


#### [[Ams.Cli.Repl.ReplState.ChapterFileComparer.GetSortKey]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static ReplState.ChapterFileComparer.SortKey GetSortKey(FileInfo file)
```

**Called-by <-**
- [[Ams.Cli.Repl.ReplState.ChapterFileComparer.Compare]]

