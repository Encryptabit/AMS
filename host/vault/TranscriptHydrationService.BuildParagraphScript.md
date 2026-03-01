---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptHydrationService::BuildParagraphScript
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`

## Summary
**Constructs a paragraph-level script string from the hydrated sentence scripts referenced by a paragraph’s sentence IDs.**

This private static aggregator builds paragraph script text by traversing ordered `sentenceIds` and resolving each entry from `sentenceMap`. It short-circuits to `string.Empty` when no sentence IDs are provided, and only appends `HydratedSentence.ScriptText` values that are present and non-whitespace (`TryGetValue` + `IsNullOrWhiteSpace` guard). The final output is a space-joined concatenation of collected sentence scripts, or `string.Empty` if nothing qualified.


#### [[TranscriptHydrationService.BuildParagraphScript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildParagraphScript(IReadOnlyList<int> sentenceIds, IReadOnlyDictionary<int, HydratedSentence> sentenceMap)
```

**Called-by <-**
- [[TranscriptHydrationService.BuildHydratedTranscriptAsync]]

