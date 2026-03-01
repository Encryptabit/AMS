---
namespace: "Ams.Cli.Workspace"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Workspace/CliWorkspace.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# CliWorkspace::NormalizeOptions
**Path**: `Projects/AMS/host/Ams.Cli/Workspace/CliWorkspace.cs`

## Summary
**Normalize and validate chapter-open options, including resolving the default book index, before opening a chapter.**

NormalizeOptions is the option-canonicalization step used by OpenChapter: it examines the incoming ChapterOpenOptions, applies defaulting/normalization branches, and delegates book selection fallback to ResolveDefaultBookIndex. Given cyclomatic complexity 6, the implementation likely contains multiple guard paths for missing or invalid fields before returning a sanitized options object. Centralizing this logic ensures OpenChapter executes against consistent, resolved option state.


#### [[CliWorkspace.NormalizeOptions]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private ChapterOpenOptions NormalizeOptions(ChapterOpenOptions options)
```

**Calls ->**
- [[CliWorkspace.ResolveDefaultBookIndex]]

**Called-by <-**
- [[CliWorkspace.OpenChapter]]

