---
namespace: "Ams.Cli.Repl"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs"
access_modifier: "public"
complexity: 6
fan_in: 3
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# ReplState::ResolveChapterFile
**Path**: `Projects/AMS/host/Ams.Cli/Repl/ReplContext.cs`

## Summary
**Construct a chapter-relative file path and optionally fail fast if the target file must already exist.**

ResolveChapterFile is the shared chapter-path resolver: it builds a FileInfo from the active chapter context plus the provided suffix, then branches on mustExist to enforce file existence when required. The method likely uses guard clauses around chapter state/suffix validity and throws on missing required artifacts, while still supporting non-existent paths for optional/probing flows. Its reuse by ResolveChapterArtifact, ResolveOutput, and TryResolveChapterArtifact keeps chapter artifact path semantics consistent across strict and best-effort callers.


#### [[ReplState.ResolveChapterFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public FileInfo ResolveChapterFile(string suffix, bool mustExist)
```

**Called-by <-**
- [[CommandInputResolver.ResolveChapterArtifact]]
- [[CommandInputResolver.ResolveOutput]]
- [[CommandInputResolver.TryResolveChapterArtifact]]

