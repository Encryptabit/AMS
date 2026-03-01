---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 3
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/error-handling
---
# MfaWorkflow::CleanupMfaArtifacts
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs`

## Summary
**It removes stale chapter MFA artifacts from the workspace before alignment while tolerating cleanup failures.**

`CleanupMfaArtifacts` performs pre-run workspace cleanup for a chapter by deleting known stale MFA outputs under `mfaRoot` only if the root directory exists. It uses a local best-effort `TryDelete` helper (with debug logging on failure) to remove chapter-specific files (`.g2p.txt`, `.dictionary.zip`, `.oov.cleaned.txt`) plus shared OOV report files, and calls `TryDeleteDirectory` for chapter-scoped directories (`.align`, `.g2p`, `.oov.cleaned`). It intentionally avoids deleting MFA’s shared `corpus` workspace to reduce contention in parallel runs.


#### [[MfaWorkflow.CleanupMfaArtifacts]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void CleanupMfaArtifacts(string mfaRoot, string chapterStem)
```

**Calls ->**
- [[TryDelete]]
- [[MfaWorkflow.TryDeleteDirectory]]
- [[Log.Debug]]

**Called-by <-**
- [[MfaWorkflow.RunChapterAsync]]

