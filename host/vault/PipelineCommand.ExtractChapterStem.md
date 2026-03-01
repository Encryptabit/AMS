---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# PipelineCommand::ExtractChapterStem
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Convert a `nameWithoutExtension` value into a canonical chapter stem by stripping pipeline-specific transcript/alignment suffixes.**

`ExtractChapterStem` normalizes a transcript-derived file stem by removing known suffixes in a strict, case-insensitive sequence. It trims `.align.tx` first (or `.tx` as a fallback), then removes a remaining `.align` suffix, using span-style string slicing (`[..^length]`) for exact-length trimming. The result is the canonical chapter stem that `ComputeChapterStats` uses to construct related hydrate, audio, and TextGrid filenames.


#### [[PipelineCommand.ExtractChapterStem]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ExtractChapterStem(string nameWithoutExtension)
```

**Called-by <-**
- [[PipelineCommand.ComputeChapterStats]]

