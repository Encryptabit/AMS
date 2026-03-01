---
namespace: "Ams.Cli.Utilities"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs"
access_modifier: "public"
complexity: 3
fan_in: 4
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# CommandInputResolver::ResolveChapterArtifact
**Path**: `Projects/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs`

## Summary
**Resolve a chapter artifact file path from user input with optional existence validation.**

`ResolveChapterArtifact` is a static resolver helper in `Ams.Cli.Utilities.CommandInputResolver` that normalizes a chapter-related `FileInfo` by delegating path resolution to `ResolveChapterFile`. It passes through the caller-supplied `suffix` and uses `mustExist` (default `true`) to enforce or relax filesystem existence checks, keeping branching low (reported complexity 3). The method serves as shared input-resolution logic for command flows like `Create`, `CreateAnchors`, `CreateHydrateTx`, and `CreateTranscriptIndex`.


#### [[CommandInputResolver.ResolveChapterArtifact]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static FileInfo ResolveChapterArtifact(FileInfo provided, string suffix, bool mustExist = true)
```

**Calls ->**
- [[ReplState.ResolveChapterFile]]

**Called-by <-**
- [[AlignCommand.CreateAnchors]]
- [[AlignCommand.CreateHydrateTx]]
- [[AlignCommand.CreateTranscriptIndex]]
- [[RefineSentencesCommand.Create]]

