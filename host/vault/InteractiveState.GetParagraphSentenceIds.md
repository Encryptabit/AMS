---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 2
fan_in: 5
fan_out: 0
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# InteractiveState::GetParagraphSentenceIds
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Return the sentence IDs associated with a paragraph so downstream timing-validation routines can operate on the correct sentence set.**

In `ValidateTimingSession.InteractiveState`, `GetParagraphSentenceIds` is a low-complexity accessor that resolves a `paragraphId` to its sentence-id collection and exposes it as `IReadOnlyList<int>` to prevent external mutation of session state. With complexity 2 and its use across markup and pause-counting flows, the implementation is effectively a lookup plus one fallback branch (for example, when no mapping exists). This makes paragraph-to-sentence retrieval consistent for `AppendParagraphMarkup`, `CollectCompressionPauses`, `CollectPauses`, `CountParagraphPauses`, and `GetParagraphSentenceCount`.


#### [[InteractiveState.GetParagraphSentenceIds]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IReadOnlyList<int> GetParagraphSentenceIds(int paragraphId)
```

**Called-by <-**
- [[InteractiveState.AppendParagraphMarkup]]
- [[InteractiveState.CollectCompressionPauses]]
- [[InteractiveState.CollectPauses]]
- [[InteractiveState.CountParagraphPauses]]
- [[InteractiveState.GetParagraphSentenceCount]]

