---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 3
fan_in: 3
fan_out: 0
tags:
  - method
  - llm/utility
---
# TimingRenderer::BuildDiffTable
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Construct and populate a reusable table view of timing diff rows for detail-level render output.**

`BuildDiffTable` is a low-complexity rendering helper that creates a `Table`, defines diff-focused columns, and adds one row per `ValidateTimingSession.DiffRow` from the provided `IReadOnlyList`. It acts as shared formatting logic for `BuildChapterDetail`, `BuildParagraphDetail`, and `BuildSentenceDetail`, so those callers compose section output while this method standardizes diff table construction.


#### [[TimingRenderer.BuildDiffTable]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Table BuildDiffTable(IReadOnlyList<ValidateTimingSession.DiffRow> diffs)
```

**Called-by <-**
- [[TimingRenderer.BuildChapterDetail]]
- [[TimingRenderer.BuildParagraphDetail]]
- [[TimingRenderer.BuildSentenceDetail]]

