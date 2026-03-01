---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs"
access_modifier: "private"
complexity: 4
fan_in: 8
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
---
# DspCommand::ResolvePath
**Path**: `Projects/AMS/host/Ams.Cli/Commands/DspCommand.cs`

## Summary
**Converts a user-provided path into a canonical absolute path, resolving relative paths against a base directory.**

`ResolvePath` validates `path` using `string.IsNullOrWhiteSpace` and throws `ArgumentException` (`nameof(path)`) when it is empty. If the input is already rooted (`Path.IsPathRooted`), it returns `Path.GetFullPath(path)` to normalize it. Otherwise it chooses `baseDirectory` (or `Directory.GetCurrentDirectory()` when blank), combines it with the relative path via `Path.Combine`, and returns the normalized absolute path with `Path.GetFullPath`.


#### [[DspCommand.ResolvePath]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ResolvePath(string path, string baseDirectory)
```

**Called-by <-**
- [[DspCommand.BuildProcessArguments]]
- [[DspCommand.BuildSingleNodeChain]]
- [[DspCommand.CreateChainListCommand]]
- [[DspCommand.CreateListParamsCommand]]
- [[DspCommand.CreateNodeFromOptions]]
- [[DspCommand.ExpandInputToken]]
- [[DspCommand.ResolveNodeOutput]]
- [[DspCommand.ResolvePluginPath]]

