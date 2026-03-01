---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 34
fan_in: 1
fan_out: 0
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/data-access
  - llm/factory
  - llm/validation
---
# ValidationReportBuilder::BuildSentenceViews
**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`

> [!danger] High Complexity (34)
> Cyclomatic complexity: 34. Consider refactoring into smaller methods.

## Summary
**Produce a normalized, ID-aligned sentence view model by combining transcript and hydrated sentence data with defined fallback rules.**

`BuildSentenceViews` merges sentence data from `TranscriptIndex` and `HydratedTranscript` by indexing each source by sentence `Id`, unioning IDs in a `SortedSet<int>`, and iterating in deterministic order. For each ID it resolves fields with hydrated-first fallback semantics (book/script ranges, metrics, status, book/script text, timing, diff), defaulting missing metrics to `new SentenceMetrics(0, 0, 0, 0, 0)` and ranges to zero/null tuples when absent. It normalizes whitespace-only text fields to `null` before constructing `SentenceView` instances. If both inputs are null it returns `Array.Empty<SentenceView>()`; otherwise it returns the built views ordered by `Id`.


#### [[ValidationReportBuilder.BuildSentenceViews]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyList<SentenceView> BuildSentenceViews(TranscriptIndex tx, HydratedTranscript hydrated)
```

**Called-by <-**
- [[ValidationReportBuilder.Build]]

