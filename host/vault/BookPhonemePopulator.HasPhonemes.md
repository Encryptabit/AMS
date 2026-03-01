---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookPhonemePopulator.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
---
# BookPhonemePopulator::HasPhonemes
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookPhonemePopulator.cs`

## Summary
**Determines if a book word already has phoneme data populated.**

`HasPhonemes` is a compact predicate that checks whether a `BookWord` already contains at least one phoneme variant. It uses C# property-pattern matching (`word.Phonemes is { Length: > 0 }`) to return `true` only when the phoneme array is non-null and non-empty. The method is pure and allocation-free.


#### [[BookPhonemePopulator.HasPhonemes]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool HasPhonemes(BookWord word)
```

**Called-by <-**
- [[BookPhonemePopulator.PopulateMissingAsync]]

