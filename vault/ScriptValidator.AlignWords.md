---
namespace: "Ams.Core.Validation"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Validation/ScriptValidator.cs"
access_modifier: "private"
complexity: 15
fan_in: 1
fan_out: 1
tags:
  - method
  - danger/high-complexity
---
# ScriptValidator::AlignWords
**Path**: `home/cari/repos/AMS/host/Ams.Core/Validation/ScriptValidator.cs`

> [!danger] High Complexity (15)
> Cyclomatic complexity: 15. Consider refactoring into smaller methods.


#### [[ScriptValidator.AlignWords]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private List<ScriptValidator.AlignmentResult> AlignWords(List<string> expected, List<ScriptValidator.WordAlignment> actual)
```

**Calls ->**
- [[ScriptValidator.CalculateMatchCost]]

**Called-by <-**
- [[ScriptValidator.Validate]]

