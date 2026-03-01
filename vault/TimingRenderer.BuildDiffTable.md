---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 3
fan_in: 3
fan_out: 0
tags:
  - method
---
# TimingRenderer::BuildDiffTable
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


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

