---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 4
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# ValidateCommand::LoadJson
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Load and deserialize a JSON file into a strongly typed model used by the validate command.**

`LoadJson<T>(FileInfo file)` is a private static generic helper in `ValidateCommand` that reads JSON from the supplied file and materializes a typed `T` instance. It follows a small branch/guard flow (complexity 4), indicating straightforward input/file-state checks plus a deserialize path, with failures surfaced as validation-friendly errors rather than raw low-level exceptions.


#### [[ValidateCommand.LoadJson]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static T LoadJson<T>(FileInfo file)
```

