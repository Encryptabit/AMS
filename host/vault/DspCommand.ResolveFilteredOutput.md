---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
---
# DspCommand::ResolveFilteredOutput
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Computes the destination file path for DSP-filtered audio using explicit, chapter-derived, and input-derived fallback rules.**

`ResolveFilteredOutput` short-circuits to `explicitOutput` when non-null, otherwise it attempts a chapter-scoped default via `CommandInputResolver.TryResolveChapterArtifact(null, "dsp.filtered.wav", mustExist: false)`. When that artifact is resolved, it proactively ensures the parent directory exists with `Directory.CreateDirectory` (guarded by `!string.IsNullOrEmpty`). If chapter resolution fails, it builds a fallback path as `<input directory or cwd>/<input stem>.filtered.wav` and returns a new `FileInfo`.


#### [[DspCommand.ResolveFilteredOutput]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FileInfo ResolveFilteredOutput(FileInfo explicitOutput, FileInfo inputFile)
```

**Calls ->**
- [[CommandInputResolver.TryResolveChapterArtifact]]

**Called-by <-**
- [[DspCommand.ExecuteFilterChain]]

