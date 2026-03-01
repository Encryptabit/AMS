---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/data-access
  - llm/error-handling
---
# ValidateCommand::SaveJson
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateCommand.cs`

## Summary
**Persist a provided payload object to a specific file as JSON.**

SaveJson<T> is a private static generic helper on ValidateCommand that takes a FileInfo destination and a typed payload, then serializes the payload to JSON at the destination path. Its reported complexity of 2 indicates very shallow control flow, typically a single guard/branch around a straightforward serialize-and-write path, with errors propagated via normal I/O or serialization exceptions.


#### [[ValidateCommand.SaveJson]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void SaveJson<T>(FileInfo destination, T payload)
```

