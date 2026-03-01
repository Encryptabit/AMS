---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 11
tags:
  - method
---
# PipelineCommand::RunVerify
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/PipelineCommand.cs`


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

