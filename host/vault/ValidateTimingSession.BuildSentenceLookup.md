---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# ValidateTimingSession::BuildSentenceLookup
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Build a read-only map from sentence identifiers to sentence text from a `BookIndex` for later validation context loading.**

`BuildSentenceLookup` is a private static helper invoked by `LoadSessionContextAsync` to convert a `BookIndex` into a sentence lookup keyed by `int`. Its implementation delegates text extraction to `ExtractBookText` and then materializes the results into an `IReadOnlyDictionary<int, string>` for fast key-based access. With cyclomatic complexity 2, it is a straightforward projection/aggregation step in the timing-validation flow.


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

