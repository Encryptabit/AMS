---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/IPronunciationProvider.cs"
access_modifier: "public"
complexity: 1
fan_in: 4
fan_out: 0
tags:
  - method
  - llm/entry-point
  - llm/async
  - llm/di
---
# IPronunciationProvider::GetPronunciationsAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/IPronunciationProvider.cs`

## Summary
**Defines the DI abstraction for asynchronously resolving pronunciation variants for a set of words.**

`GetPronunciationsAsync` is the sole asynchronous contract method on `IPronunciationProvider`, declared as `Task<IReadOnlyDictionary<string, string[]>> GetPronunciationsAsync(IEnumerable<string> words, CancellationToken cancellationToken)`. It defines a lookup API that maps normalized word keys to one-or-more pronunciation variants (`string[]`) without prescribing source/backing implementation details.


#### [[IPronunciationProvider.GetPronunciationsAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
Task<IReadOnlyDictionary<string, string[]>> GetPronunciationsAsync(IEnumerable<string> words, CancellationToken cancellationToken)
```

**Called-by <-**
- [[BookPhonemePopulator.PopulateMissingAsync]]
- [[BookIndexer.CreateIndexAsync]]
- [[TranscriptHydrationService.BuildPronunciationFallbackAsync]]
- [[TranscriptIndexService.BuildAsrPhonemeViewAsync]]

