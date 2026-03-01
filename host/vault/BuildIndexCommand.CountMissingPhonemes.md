---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# BuildIndexCommand::CountMissingPhonemes
**Path**: `Projects/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs`

## Summary
**Count words in a BookIndex that are eligible for lookup but currently have no phoneme sequence.**

CountMissingPhonemes computes missing pronunciation coverage by running a LINQ Count over index.Words with a compound predicate. It counts a word only if its Phonemes field is null or empty (`is not { Length: > 0 }`) and PronunciationHelper.NormalizeForLookup(word.Text) returns a non-empty string, which filters out non-lookuppable tokens. The method is a pure static O(n) helper used by EnsurePhonemesAsync to compare pre/post enrichment results.


#### [[BuildIndexCommand.CountMissingPhonemes]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static int CountMissingPhonemes(BookIndex index)
```

**Calls ->**
- [[PronunciationHelper.NormalizeForLookup]]

**Called-by <-**
- [[BuildIndexCommand.EnsurePhonemesAsync]]

