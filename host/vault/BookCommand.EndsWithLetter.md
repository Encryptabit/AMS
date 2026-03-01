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
# BookCommand::EndsWithLetter
**Path**: `Projects/AMS/host/Ams.Cli/Commands/BookCommand.cs`

## Summary
**Determines whether a token ends with a letter so verification logic can identify contraction/apostrophe split artifacts.**

`EndsWithLetter` is an expression-bodied helper that returns `true` only when `s` is not null/empty and `char.IsLetter(s[^1])` succeeds on the final character. It relies on short-circuit evaluation to guard the index-from-end access (`s[^1]`) from empty inputs. In `RunVerifyAsync`, it is paired with `IsContractionSuffix(cur)` to flag likely apostrophe tokenization splits (for example, `word` followed by `'s`).


#### [[BookCommand.EndsWithLetter]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool EndsWithLetter(string s)
```

**Called-by <-**
- [[BookCommand.RunVerifyAsync]]

