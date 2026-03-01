---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 2
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
---
# Program::ReplacePlaceholders
**Path**: `Projects/AMS/host/Ams.Cli/Program.cs`

## Summary
**Resolve known chapter/audio placeholders in command arguments into concrete values for a specific `FileInfo` chapter.**

`ReplacePlaceholders` creates a new `string[]` matching `args.Length` and performs a single pass over inputs. Each element is transformed via chained case-insensitive `String.Replace` calls, mapping `{audio}` to `chapter.FullName` and both `{chapter}` and `{chapterName}` to `Path.GetFileNameWithoutExtension(chapter.Name)`. It leaves the original `args` untouched and returns the per-chapter concrete argument list consumed by the chapter execution paths.


#### [[Program.ReplacePlaceholders]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string[] ReplacePlaceholders(string[] args, FileInfo chapter)
```

**Called-by <-**
- [[Program.ExecuteChaptersInParallelAsync]]
- [[Program.ExecuteWithScopeAsync]]

