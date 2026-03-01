---
namespace: "Ams.Cli.Workspace"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Workspace/CliWorkspace.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/factory
  - llm/validation
  - llm/utility
---
# CliWorkspace::OpenChapter
**Path**: `Projects/AMS/host/Ams.Cli/Workspace/CliWorkspace.cs`

## Summary
**Open a chapter context for the CLI workspace by standardizing open options and creating the corresponding context handle.**

OpenChapter(ChapterOpenOptions options) in CliWorkspace is a thin coordinator that normalizes the incoming options via NormalizeOptions, then constructs/opens the chapter context via CreateContext, and returns a ChapterContextHandle. With complexity 2, the implementation is intentionally minimal and likely limited to delegation plus a small guard/branch.


#### [[CliWorkspace.OpenChapter]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ChapterContextHandle OpenChapter(ChapterOpenOptions options)
```

**Calls ->**
- [[CliWorkspace.NormalizeOptions]]
- [[ChapterManager.CreateContext]]

