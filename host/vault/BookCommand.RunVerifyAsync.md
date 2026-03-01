---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/BookCommand.cs"
access_modifier: "private"
complexity: 66
fan_in: 1
fan_out: 6
tags:
  - method
  - danger/high-complexity
  - llm/entry-point
  - llm/data-access
  - llm/async
  - llm/validation
  - llm/error-handling
---
# BookCommand::RunVerifyAsync
**Path**: `Projects/AMS/host/Ams.Cli/Commands/BookCommand.cs`

> [!danger] High Complexity (66)
> Cyclomatic complexity: 66. Consider refactoring into smaller methods.

## Summary
**Performs non-mutating integrity verification of a `BookIndex` file and reports deterministic validation diagnostics for command/CI execution.**

`RunVerifyAsync` asynchronously reads the supplied index JSON file, deserializes it into `BookIndex` using tolerant `JsonSerializerOptions` (camelCase, comments skipped, trailing commas allowed), and throws `FileNotFoundException`/`InvalidOperationException` on missing or unparsable canonical data. It validates core invariants: totals parity vs array lengths, contiguous and in-range sentence/paragraph spans, sequential indices, and per-word sentence/paragraph back-references. It also emits heuristic warnings for apostrophe tokenization splits (`EndsWithLetter`, `IsContractionSuffix`, `IsStandaloneApostrophe`) and TOC-like paragraph bursts based on per-paragraph sentence-length `Median`, then computes a canonical JSON SHA-256 (`Sha256Hex`) and sets `Environment.ExitCode = 2` if failures are present.


#### [[BookCommand.RunVerifyAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task RunVerifyAsync(FileInfo indexFile)
```

**Calls ->**
- [[BookCommand.EndsWithLetter]]
- [[BookCommand.IsContractionSuffix]]
- [[BookCommand.IsStandaloneApostrophe]]
- [[BookCommand.Median]]
- [[BookCommand.Sha256Hex]]
- [[Log.Debug]]

**Called-by <-**
- [[BookCommand.CreateVerify]]

