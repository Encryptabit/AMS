---
namespace: "Ams.Core.Application.Validation"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs"
access_modifier: "private"
complexity: 24
fan_in: 1
fan_out: 6
tags:
  - method
  - danger/high-complexity
  - llm/utility
  - llm/validation
---
# ValidationReportBuilder::BuildTextReport
**Path**: `Projects/AMS/host/Ams.Core/Application/Validation/ValidationReportBuilder.cs`

> [!danger] High Complexity (24)
> Cyclomatic complexity: 24. Consider refactoring into smaller methods.

## Summary
**Generate the human-readable validation report string for sentence/paragraph diff quality and related metadata.**

`BuildTextReport` composes a plain-text validation report with a `StringBuilder`, writing source metadata, sentence/paragraph aggregate diff totals from `AggregateDiffStats` + `FormatDiffTotals`, and optional `WordTallies` counts. It conditionally emits sentence and paragraph detail sections from `ValidationReportOptions`, sorting by computed diff score (then ref-token count, then ID) and choosing either top-N or only issue-bearing entries (`AllErrors`). For each selected view it prints per-item stats/status, trimmed text (`TrimText`), and sampled non-`equal` diff operations via `AppendDiffOps`; when `IncludeAllFlagged` is enabled, paragraph inclusion is expanded by projecting flagged sentence IDs through `hydrated.Paragraphs`. The method returns the final text with trailing whitespace removed via `TrimEnd()`.


#### [[ValidationReportBuilder.BuildTextReport]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildTextReport(SourceInfo info, IReadOnlyList<SentenceView> sentences, IReadOnlyList<ParagraphView> paragraphs, WordTallies wordTallies, ValidationReportOptions options, HydratedTranscript hydrated)
```

**Calls ->**
- [[ValidationReportBuilder.AggregateDiffStats]]
- [[ValidationReportBuilder.AppendDiffOps]]
- [[ValidationReportBuilder.FormatDiffStats]]
- [[ValidationReportBuilder.FormatDiffTotals]]
- [[ValidationReportBuilder.HasParagraphDiffIssues]]
- [[ValidationReportBuilder.TrimText]]

**Called-by <-**
- [[ValidationReportBuilder.Build]]

