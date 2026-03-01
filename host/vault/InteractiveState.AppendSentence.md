---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
---
# InteractiveState::AppendSentence
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Append sentence-level validation scope data for a paragraph by transforming a `SentencePauseMap` into labeled `ScopeEntry` data.**

`AppendSentence` is a helper in `InteractiveState` that mutates the provided `List<ValidateTimingSession.ScopeEntry>` by projecting a `SentencePauseMap` into sentence-scoped entries associated with `paragraphId`. It delegates label construction to `BuildPauseLabel(sentence)` and `BuildSentenceLabel(sentence)`, so formatting/presentation logic stays centralized instead of being duplicated in this method. Given its low complexity (3) and its use from `AppendParagraph`, it serves as the sentence-level expansion step in paragraph traversal during interactive timing validation.


#### [[InteractiveState.AppendSentence]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void AppendSentence(List<ValidateTimingSession.ScopeEntry> entries, int paragraphId, SentencePauseMap sentence)
```

**Calls ->**
- [[InteractiveState.BuildPauseLabel]]
- [[InteractiveState.BuildSentenceLabel]]

**Called-by <-**
- [[InteractiveState.AppendParagraph]]

