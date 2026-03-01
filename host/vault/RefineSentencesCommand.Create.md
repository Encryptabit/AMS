---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/RefineSentencesCommand.cs"
access_modifier: "public"
complexity: 1
fan_in: 1
fan_out: 4
tags:
  - method
  - llm/entry-point
  - llm/factory
  - llm/async
  - llm/validation
  - llm/error-handling
---
# RefineSentencesCommand::Create
**Path**: `Projects/AMS/host/Ams.Cli/Commands/RefineSentencesCommand.cs`

## Summary
**Construct and return the sentence-refinement CLI command with its validation, artifact resolution, async execution, and error handling pipeline.**

`Create` is a static factory that constructs the `RefineSentencesCommand` CLI `Command` and wires its execution flow in a single linear path (complexity 1). Its handler delegates to `RequireAudio` for preconditions, `ResolveChapterArtifact` for input/artifact lookup, and `RunAsync` for the actual asynchronous refinement work, with failures surfaced via `Error`. Since it is called by `Main`, this method acts as the registration boundary between app startup and command runtime behavior.


#### [[RefineSentencesCommand.Create]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Command Create()
```

**Calls ->**
- [[RefineSentencesCommand.RunAsync]]
- [[CommandInputResolver.RequireAudio]]
- [[CommandInputResolver.ResolveChapterArtifact]]
- [[Log.Error]]

**Called-by <-**
- [[Program.Main]]

