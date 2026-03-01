---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 5
fan_in: 2
fan_out: 0
tags:
  - method
---
# ValidateTimingSession::ExtractBookText
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[ValidateTimingSession.ExtractBookText]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string ExtractBookText(BookIndex book, int start, int end)
```

**Called-by <-**
- [[ValidateTimingSession.BuildParagraphData]]
- [[ValidateTimingSession.BuildSentenceLookup]]

