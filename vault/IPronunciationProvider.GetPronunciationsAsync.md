---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/IPronunciationProvider.cs"
access_modifier: "public"
complexity: 1
fan_in: 4
fan_out: 0
tags:
  - method
---
# IPronunciationProvider::GetPronunciationsAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/IPronunciationProvider.cs`


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

