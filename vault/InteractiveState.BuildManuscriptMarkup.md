---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "public"
complexity: 15
fan_in: 1
fan_out: 3
tags:
  - method
  - danger/high-complexity
---
# InteractiveState::BuildManuscriptMarkup
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

> [!danger] High Complexity (15)
> Cyclomatic complexity: 15. Consider refactoring into smaller methods.


#### [[InteractiveState.BuildManuscriptMarkup]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public string BuildManuscriptMarkup(ValidateTimingSession.ScopeEntry entry)
```

**Calls ->**
- [[InteractiveState.AppendChapterPreview]]
- [[InteractiveState.AppendParagraphMarkup]]
- [[InteractiveState.AppendPauseSentencesFallback]]

**Called-by <-**
- [[TimingRenderer.BuildManuscript]]

