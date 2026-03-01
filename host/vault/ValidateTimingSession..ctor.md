---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 10
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/entry-point
  - llm/di
  - llm/validation
  - llm/error-handling
---
# ValidateTimingSession::.ctor
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Configure and bootstrap a timing-validation command session using workspace-resolved services, input artifacts, and runtime analysis flags.**

The `ValidateTimingSession` constructor initializes a CLI timing-validation session from an `IWorkspace` and three required `FileInfo` inputs (`transcriptFile`, `bookIndexFile`, `hydrateFile`), while storing behavior switches for prosody and gap filtering (`runProsodyAnalysis`, `includeAllIntraSentenceGaps`, `interSentenceOnly`). It emits diagnostic traces through `Debug` and composes dependencies via `Resolve`, so setup is DI-driven rather than hard-wired. The reported complexity (10) indicates non-trivial branching in constructor setup based on option combinations and resolved services.


#### [[ValidateTimingSession..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public ValidateTimingSession(IWorkspace workspace, FileInfo transcriptFile, FileInfo bookIndexFile, FileInfo hydrateFile, bool runProsodyAnalysis, bool includeAllIntraSentenceGaps = false, bool interSentenceOnly = true)
```

**Calls ->**
- [[PausePolicyResolver.Resolve]]
- [[Log.Debug]]

