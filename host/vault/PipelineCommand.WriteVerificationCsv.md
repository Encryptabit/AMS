---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/utility
---
# PipelineCommand::WriteVerificationCsv
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Generate a CSV verification report of audio mismatches for a chapter/variant from an `AudioVerificationResult`.**

Writes a UTF-8 CSV report to `path` by recreating the file (`new StreamWriter(path, false, Encoding.UTF8)`), emitting a fixed header, and iterating `result.Mismatches` to output one row per mismatch. It derives `sentenceIds` by joining `mismatch.Sentences[*].SentenceId` with `|` (or empty if none), formats times as invariant-culture `F6` and dB values as invariant-culture `F2`, and uses `EscapeCsv` for `chapterLabel`, `variantLabel`, and the sentence-id field.


#### [[PipelineCommand.WriteVerificationCsv]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void WriteVerificationCsv(string path, string chapterLabel, string variantLabel, AudioVerificationResult result)
```

**Calls ->**
- [[PipelineCommand.EscapeCsv]]

**Called-by <-**
- [[PipelineCommand.RunVerify]]

