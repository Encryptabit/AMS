---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookPhonemePopulator.cs"
access_modifier: "public"
complexity: 14
fan_in: 1
fan_out: 5
tags:
  - method
---
# BookPhonemePopulator::PopulateMissingAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookPhonemePopulator.cs`


#### [[BookPhonemePopulator.PopulateMissingAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static Task<BookIndex> PopulateMissingAsync(BookIndex index, IPronunciationProvider pronunciationProvider, CancellationToken cancellationToken = default(CancellationToken))
```

**Calls ->**
- [[Log.Debug]]
- [[BookPhonemePopulator.HasPhonemes]]
- [[BookPhonemePopulator.MergeVariants]]
- [[IPronunciationProvider.GetPronunciationsAsync]]
- [[PronunciationHelper.NormalizeForLookup]]

**Called-by <-**
- [[DocumentProcessor.PopulateMissingPhonemesAsync]]

