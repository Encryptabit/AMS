---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# MfaWorkflow::PrepareLabLines
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs`

## Summary
**It filters and normalizes raw text lines into a clean list of usable LAB corpus lines.**

`PrepareLabLines` transforms an enumerable of raw transcript lines into MFA-ready corpus lines by iterating each input, normalizing it through `PrepareLabLine`, and collecting only non-empty/non-whitespace results. It preserves input order and returns a mutable `List<string>` containing validated lines suitable for downstream lab-file writing.


#### [[MfaWorkflow.PrepareLabLines]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static List<string> PrepareLabLines(IEnumerable<string> rawLines)
```

**Calls ->**
- [[MfaWorkflow.PrepareLabLine]]

**Called-by <-**
- [[MfaWorkflow.WriteLabFileAsync]]

