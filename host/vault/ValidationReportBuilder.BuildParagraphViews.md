---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 5
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/factory
  - llm/validation
---
# ValidationReportBuilder::BuildParagraphViews
**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`

## Summary
**Create a normalized paragraph view collection from hydrated data when available, or from transcript paragraphs as a fallback.**

`BuildParagraphViews` returns `Array.Empty<ParagraphView>()` when both sources are null; otherwise it prefers `hydrated.Paragraphs` and falls back to projecting `tx.Paragraphs` into `HydratedParagraph` shims (empty `BookText`, `Diff: null`) when hydrated data is absent. It then maps each paragraph into `ParagraphView`, carrying id/book-range/metrics/status/diff and normalizing whitespace-only `BookText` to `null`. The final sequence is ordered by paragraph `Id` and materialized as a list.


#### [[ValidationReportBuilder.BuildParagraphViews]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyList<ParagraphView> BuildParagraphViews(TranscriptIndex tx, HydratedTranscript hydrated)
```

**Called-by <-**
- [[ValidationReportBuilder.Build]]

