---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 2
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# DspCommand::ResolveOutputFile
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Determine the DSP output file path by preferring explicit input, then context-based resolution, then a source-directory naming fallback.**

`ResolveOutputFile` first returns the explicit `provided` path when present. If no path is provided and `DspSessionState.OutputMode` is `Post`, it attempts `CommandInputResolver.ResolveOutput(null, "dsp.wav")` to derive an output from active context, but catches any exception and logs a debug message before falling back. The fallback constructs a `FileInfo` in `inputFile.DirectoryName` (or `Directory.GetCurrentDirectory()`) using the input stem and suffix `.treated.wav` for `Source` mode or `.dsp.wav` otherwise.


#### [[DspCommand.ResolveOutputFile]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static FileInfo ResolveOutputFile(FileInfo provided, FileInfo inputFile)
```

**Calls ->**
- [[CommandInputResolver.ResolveOutput]]
- [[Log.Debug]]

**Called-by <-**
- [[DspCommand.CreateRunCommand]]

