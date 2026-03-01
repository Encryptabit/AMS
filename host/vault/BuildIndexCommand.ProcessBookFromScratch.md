---
namespace: "Ams.Cli.Commands"
project: "Ams.Cli"
source_file: "Projects/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 4
tags:
  - method
  - llm/async
  - llm/data-access
  - llm/di
  - llm/utility
---
# BuildIndexCommand::ProcessBookFromScratch
**Path**: `Projects/AMS/host/Ams.Cli/Commands/BuildIndexCommand.cs`

## Summary
**Construct a new book index from source content and optionally write it to cache.**

`ProcessBookFromScratch` asynchronously rebuilds a `BookIndex` end-to-end by calling `DocumentProcessor.ParseBookAsync` on `bookFilePath`, then `DocumentProcessor.BuildBookIndexAsync` with the parse result, `BookIndexOptions`, injected `IPronunciationProvider`, and the same `CancellationToken`. It wraps each stage with `Log.Debug` messages for parse/build/cache progress visibility. When `cache` is non-null, it persists the result via `cache.SetAsync`; otherwise caching is skipped. It returns the built `BookIndex` directly, with no local validation or exception translation.


#### [[BuildIndexCommand.ProcessBookFromScratch]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static Task<BookIndex> ProcessBookFromScratch(IBookCache cache, IPronunciationProvider pronunciationProvider, string bookFilePath, BookIndexOptions options, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[Log.Debug]]
- [[DocumentProcessor.BuildBookIndexAsync]]
- [[DocumentProcessor.ParseBookAsync]]
- [[IBookCache.SetAsync]]

**Called-by <-**
- [[BuildIndexCommand.BuildBookIndexAsync]]

