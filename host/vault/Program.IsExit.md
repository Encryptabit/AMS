---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# Program::IsExit
**Path**: `Projects/AMS/host/Ams.Cli/Program.cs`

## Summary
**Determines whether the provided REPL input should trigger application exit.**

`IsExit(string input)` is a small REPL helper used by `StartRepl` to decide whether a user-entered line should terminate the interaction loop. Given cyclomatic complexity 3, the implementation likely combines basic input normalization/guard checks with one or more explicit comparisons against supported exit tokens.


#### [[Program.IsExit]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsExit(string input)
```

**Called-by <-**
- [[Program.StartRepl]]

