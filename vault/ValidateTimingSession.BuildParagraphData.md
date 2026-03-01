---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
---
# ValidateTimingSession::BuildParagraphData
**Path**: `home/cari/repos/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`


#### [[ValidateTimingSession.BuildParagraphData]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (IReadOnlyList<ValidateTimingSession.ParagraphInfo> Paragraphs, IReadOnlyDictionary<int, int> SentenceToParagraph, IReadOnlyDictionary<int, IReadOnlyList<int>> ParagraphSentences) BuildParagraphData(BookIndex book)
```

**Calls ->**
- [[ValidateTimingSession.ExtractBookText]]

**Called-by <-**
- [[ValidateTimingSession.LoadSessionContextAsync]]

