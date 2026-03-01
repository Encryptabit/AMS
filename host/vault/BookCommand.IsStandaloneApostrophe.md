---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/BookCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookCommand::IsStandaloneApostrophe
**Path**: `Projects/AMS/host/Ams.Cli/Commands/BookCommand.cs`

## Summary
**Identify tokens that are exactly an apostrophe character so verification can flag probable tokenizer apostrophe splits.**

In `Ams.Cli.Commands.BookCommand`, `IsStandaloneApostrophe` is a private static, expression-bodied helper used inside `RunVerifyAsync`’s word-pair scan for apostrophe-split heuristics. The method performs exact token matching and returns `true` only for a single straight apostrophe (`'`) or single curly apostrophe (`’`), keeping standalone punctuation detection separate from contraction suffix detection in `IsContractionSuffix`.


#### [[BookCommand.IsStandaloneApostrophe]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool IsStandaloneApostrophe(string s)
```

**Called-by <-**
- [[BookCommand.RunVerifyAsync]]

