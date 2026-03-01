---
namespace: "Ams.Core.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# ScriptValidator::GetTranscriptText
**Path**: `Projects/AMS/host/Ams.Core/Validation/ScriptValidator.cs`

## Summary
**Build a single space-delimited transcript string from an ASR response’s word collection for validation computations.**

`GetTranscriptText` is a private helper in `ScriptValidator` that materializes transcript text by calling `string.Join(" ", asrResponse.Words)`. It is a direct pass-through over `AsrResponse.Words` with no normalization, filtering, or defensive checks, so output strictly reflects token order/content. In `Validate`, its result is used as the `actual` input for character-error-rate calculation.


#### [[ScriptValidator.GetTranscriptText]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private string GetTranscriptText(AsrResponse asrResponse)
```

**Called-by <-**
- [[ScriptValidator.Validate]]

