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
# MfaWorkflow::PrepareLabLine
**Path**: `Projects/AMS/host/Ams.Core/Application/Mfa/MfaWorkflow.cs`

## Summary
**It converts raw text into a sanitized, tokenized LAB line or an empty string when no usable pronunciation tokens exist.**

`PrepareLabLine` normalizes a single transcript line into MFA-compatible token text. It returns `string.Empty` for null/whitespace input, otherwise calls `PronunciationHelper.ExtractPronunciationParts(text)` and returns `string.Empty` when no parts are produced. For valid output it joins extracted parts with single spaces (`string.Join(' ', parts)`), yielding a cleaned pronunciation-oriented line.


#### [[MfaWorkflow.PrepareLabLine]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string PrepareLabLine(string text)
```

**Calls ->**
- [[PronunciationHelper.ExtractPronunciationParts]]

**Called-by <-**
- [[MfaWorkflow.PrepareLabLines]]

