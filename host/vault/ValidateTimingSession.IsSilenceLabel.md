---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# ValidateTimingSession::IsSilenceLabel
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Identify whether a TextGrid token should be treated as a silence label during MFA silence-span loading.**

This private static predicate classifies MFA label text by first treating null/whitespace as silence, then trimming and evaluating a `switch` on `text.Length`. It returns `true` for empty labels, `"sp"` (length 2), `"sil"` (length 3), and `<sil>` (case-insensitive `OrdinalIgnoreCase` checks), and `false` for anything else. `TryLoadMfaSilences` uses it to filter parsed TextGrid word intervals into silence spans.


#### [[ValidateTimingSession.IsSilenceLabel]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsSilenceLabel(string text)
```

**Called-by <-**
- [[ValidateTimingSession.TryLoadMfaSilences]]

