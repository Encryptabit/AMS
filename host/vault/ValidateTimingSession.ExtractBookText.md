---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 5
fan_in: 2
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# ValidateTimingSession::ExtractBookText
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Extract a valid text segment from a `BookIndex` using `start` and `end` offsets for downstream timing-session validation flows.**

`ExtractBookText` is a private static helper in `Ams.Cli.Commands.ValidateTimingSession` that is called by `BuildParagraphData` and `BuildSentenceLookup` to convert offset spans into actual text. It applies guard logic around `BookIndex` and the `start`/`end` bounds, normalizes or rejects invalid ranges, then returns the extracted substring. The reported complexity of 5 aligns with multiple branch checks before performing the slice operation.


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

