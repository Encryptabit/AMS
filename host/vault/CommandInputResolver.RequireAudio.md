---
namespace: "Ams.Cli.Utilities"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs"
access_modifier: "public"
complexity: 4
fan_in: 7
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# CommandInputResolver::RequireAudio
**Path**: `Projects/AMS/host/Ams.Cli/Utilities/CommandInputResolver.cs`

## Summary
**Validate and return a provided audio file handle so downstream command creation executes only with a valid audio input.**

RequireAudio is a static guard utility in `CommandInputResolver` that normalizes a caller-supplied `FileInfo` and enforces audio-input preconditions before command construction. With cyclomatic complexity 4, the method is likely a short validation chain (for example null/path/existence/audio-type checks) that fails fast via exceptions and returns the validated `FileInfo` for continued use. Its reuse across multiple `Create*` entry paths indicates it centralizes input contract enforcement and avoids duplicated CLI argument validation logic.


#### [[CommandInputResolver.RequireAudio]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static FileInfo RequireAudio(FileInfo provided)
```

**Called-by <-**
- [[AlignCommand.CreateTranscriptIndex]]
- [[AsrCommand.Create]]
- [[DspCommand.CreateFilterChainRunCommand]]
- [[DspCommand.CreateRunCommand]]
- [[DspCommand.CreateTestAllCommand]]
- [[PipelineCommand.CreateRun]]
- [[RefineSentencesCommand.Create]]

