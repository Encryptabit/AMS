---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
---
# ValidateTimingSession::BuildSentenceLookup
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[ValidateTimingSession.BuildSentenceLookup]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static IReadOnlyDictionary<int, string> BuildSentenceLookup(BookIndex book)
```

**Calls ->**
- [[ValidateTimingSession.ExtractBookText]]

**Called-by <-**
- [[ValidateTimingSession.LoadSessionContextAsync]]

