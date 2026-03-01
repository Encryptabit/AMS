---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# MfaWorkflow::CreateSanitizedOovList
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs`

## Summary
**It sanitizes and deduplicates a raw MFA OOV list into a cleaned chapter-specific file for downstream G2P processing.**

`CreateSanitizedOovList` builds a chapter-scoped cleaned OOV file path (`{chapterStem}.oov.cleaned.txt` under `mfaRoot`) and processes `rawOovPath` line-by-line via `File.ReadLines`. It normalizes each token by removing BOM chars, trimming whitespace and quote-like delimiters, filters out empty/non-alphabetic entries, and deduplicates with a case-insensitive `HashSet<string>`. If at least one token remains, it writes a case-insensitive sorted list to disk and returns the cleaned path; otherwise it returns `null`. Any exception is caught, logged with `Log.Debug`, and results in `null`.


#### [[MfaWorkflow.CreateSanitizedOovList]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string CreateSanitizedOovList(string mfaRoot, string chapterStem, string rawOovPath)
```

**Calls ->**
- [[Log.Debug]]

**Called-by <-**
- [[MfaWorkflow.RunChapterAsync]]

