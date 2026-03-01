---
namespace: "Ams.Core.Services.Alignment"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# TranscriptHydrationService::JoinBook
**Path**: `Projects/AMS/host/Ams.Core/Services/Alignment/TranscriptHydrationService.cs`

## Summary
**Returns normalized book text for a specified inclusive word-index range.**

`JoinBook` assembles a contiguous book text segment from `book.Words` using an inclusive start/end word range. It guards invalid ranges (`start < 0`, `end >= book.Words.Length`, or `end < start`) by returning `string.Empty`. For valid input it joins selected `Word.Text` values with spaces (`Skip/Take` + `string.Join`) and normalizes the result via `NormalizeSurface`.


#### [[TranscriptHydrationService.JoinBook]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string JoinBook(BookIndex book, int start, int end)
```

**Calls ->**
- [[TranscriptHydrationService.NormalizeSurface]]

**Called-by <-**
- [[TranscriptHydrationService.BuildHydratedTranscriptAsync]]

