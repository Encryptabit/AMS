---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookPhonemePopulator.cs"
access_modifier: "public"
complexity: 14
fan_in: 1
fan_out: 5
tags:
  - method
  - llm/async
  - llm/utility
  - llm/factory
  - llm/di
  - llm/validation
---
# BookPhonemePopulator::PopulateMissingAsync
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookPhonemePopulator.cs`

## Summary
**Asynchronously populates missing word phonemes in a book index using a pronunciation provider while preserving existing phoneme data.**

`PopulateMissingAsync` enriches a `BookIndex` by filling phoneme variants only for words lacking phonemes, after normalizing tokens with `PronunciationHelper.NormalizeForLookup`. It validates `pronunciationProvider`, gathers unique missing lexemes into a case-insensitive set, short-circuits on empty work, then performs a single async `GetPronunciationsAsync` lookup. It builds a new `Words` array, updating matched words with `word with { Phonemes = MergeVariants(...) }` (deduped/trimmed and capped by `MaxPhonemeVariantsPerWord`), and returns an updated index record.


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

