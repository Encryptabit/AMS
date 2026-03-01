---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 11
tags:
  - method
  - llm/data-access
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# PipelineCommand::RunVerify
**Path**: `Projects/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`

## Summary
**Verifies processed chapter audio variants against raw chapter audio and timing metadata, then reports mismatches and artifacts in table/log/report-file form.**

`RunVerify` enumerates verification targets from `ResolveVerifyTargets`, then drives a live Spectre.Console table while processing each chapter/variant with `cancellationToken.ThrowIfCancellationRequested()`. It validates required inputs (hydrate JSON and processed variant WAVs), decodes raw/variant audio (`AudioProcessor.Decode` + `ToMono`), loads sentence timings (`LoadSentenceTimings`), and computes integrity deltas via `AudioIntegrityVerifier.Verify` using `windowMs`, `stepMs`, `minDurationMs`, and `mergeGapMs`. Per variant, it ensures an output directory, normalizes variant naming, writes JSON/CSV reports (`JsonSerializer.Serialize` / `WriteVerificationCsv`), updates processed/issue/skipped counters, and logs detailed debug/error outcomes with nested exception handling.


#### [[PipelineCommand.RunVerify]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void RunVerify(DirectoryInfo root, DirectoryInfo reportDir, string chapterName, bool verifyAll, PipelineCommand.VerificationReportFormat format, double windowMs, double stepMs, double minDurationMs, double mergeGapMs, CancellationToken cancellationToken)
```

**Calls ->**
- [[PipelineCommand.EnsureDirectory]]
- [[PipelineCommand.LoadSentenceTimings]]
- [[PipelineCommand.NormalizeVariantToken]]
- [[PipelineCommand.ResolveProcessedVariants]]
- [[PipelineCommand.ResolveVerifyTargets]]
- [[PipelineCommand.ToMono]]
- [[PipelineCommand.WriteVerificationCsv]]
- [[AudioIntegrityVerifier.Verify]]
- [[Log.Debug]]
- [[Log.Error]]
- [[AudioProcessor.Decode]]

**Called-by <-**
- [[PipelineCommand.CreateVerifyCommand]]

