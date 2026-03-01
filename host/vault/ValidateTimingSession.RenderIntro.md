---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# ValidateTimingSession::RenderIntro
**Path**: `Projects/AMS/host/Ams.Cli/Commands/ValidateTimingSession.cs`

## Summary
**Print the startup banner and key input/analysis counts for a timing-validation session.**

`RenderIntro` is a synchronous console-output helper called from `RunAsync` before interactive processing begins. It uses Spectre.Console (`AnsiConsole.WriteLine`, `MarkupLine`, and `MarkupLineInterpolated`) to print a "Timing session" header, the `_transcriptFile` and `_bookIndexFile` paths, and conditionally the `_hydrateFile` path when present. It then reports loaded counts from `transcript.Sentences.Count`, the passed `gapCount`, and `book.Words.Length`, with no side effects beyond terminal rendering.


#### [[ValidateTimingSession.RenderIntro]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private void RenderIntro(TranscriptIndex transcript, BookIndex book, int gapCount)
```

**Called-by <-**
- [[ValidateTimingSession.RunAsync]]

