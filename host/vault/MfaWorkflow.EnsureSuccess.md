---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs"
access_modifier: "private"
complexity: 7
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# MfaWorkflow::EnsureSuccess
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs`

## Summary
**It centralizes MFA command result validation by logging output and enforcing failure policy through exception or boolean signaling.**

`EnsureSuccess` logs all `result.StdOut`/`StdErr` lines with stage-prefixed debug entries and logs the executed command, then evaluates `result.ExitCode`. When the exit code is non-zero and `allowFailure` is `false`, it builds a detailed failure message (including command, exit code, and truncated stdout/stderr snippets via local `FormatLines`) and throws `InvalidOperationException`; when `allowFailure` is `true`, it logs and returns `false` instead. It returns `true` only for zero exit codes.


#### [[MfaWorkflow.EnsureSuccess]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool EnsureSuccess(string stage, MfaCommandResult result, bool allowFailure = false)
```

**Calls ->**
- [[FormatLines]]
- [[Log.Debug]]

**Called-by <-**
- [[MfaWorkflow.RunChapterAsync]]

