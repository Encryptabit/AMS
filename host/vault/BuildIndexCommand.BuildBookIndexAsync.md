---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs"
access_modifier: "internal"
complexity: 10
fan_in: 1
fan_out: 9
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/validation
  - llm/utility
---
# BuildIndexCommand::BuildBookIndexAsync
**Path**: `Projects/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs`

## Summary
**Build and optionally cache a book index file, reusing existing cache when allowed and regenerating from source when required.**

BuildBookIndexAsync is an async orchestration method that drives index creation for a single book file with cache-aware control flow. It validates the input using CanParseBook and supported-extension checks from GetSupportedBookExtensions, initializes cache state via CreateBookCache, then branches between cached retrieval (GetAsync) and full regeneration (EnsurePhonemesAsync + ProcessBookFromScratch) based on forceRefresh/noCache. It persists rebuilt output with SetAsync and emits timing/diagnostic information through Debug and FormatDuration.


#### [[BuildIndexCommand.BuildBookIndexAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
internal static Task BuildBookIndexAsync(FileInfo bookFile, FileInfo outputFile, bool forceRefresh, BookIndexOptions options, bool noCache)
```

**Calls ->**
- [[BuildIndexCommand.EnsurePhonemesAsync]]
- [[BuildIndexCommand.FormatDuration]]
- [[BuildIndexCommand.ProcessBookFromScratch]]
- [[Log.Debug]]
- [[DocumentProcessor.CanParseBook]]
- [[DocumentProcessor.CreateBookCache]]
- [[DocumentProcessor.GetSupportedBookExtensions]]
- [[IBookCache.GetAsync]]
- [[IBookCache.SetAsync]]

**Called-by <-**
- [[BuildIndexCommand.Create]]

