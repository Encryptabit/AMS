---
namespace: "Ams.Cli"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Program.cs"
access_modifier: "private"
complexity: 6
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
---
# Program::ParseInput
**Path**: `Projects/AMS/host/Ams.Cli/Program.cs`

## Summary
**Convert a raw REPL command line into a `string[]` of arguments for `StartRepl` execution and `TryHandleBuiltInAsync` built-in command dispatch.**

`ParseInput` performs a single-pass character scan to tokenize REPL input into argv-style arguments, using a `StringBuilder` for the current token and a `List<string>` for results. It toggles an `inQuotes` flag on each `"` and treats spaces as delimiters only when `inQuotes` is false, so quoted spans preserve internal spaces. Tokens are flushed on delimiter and once at end-of-input, and returned via `args.ToArray()`. The parser strips quote characters and does not handle escaped quotes or unmatched-quote validation (best-effort parsing).


#### [[Program.ParseInput]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string[] ParseInput(string input)
```

**Called-by <-**
- [[Program.StartRepl]]
- [[Program.TryHandleBuiltInAsync]]

