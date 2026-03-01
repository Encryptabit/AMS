---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookPhonemePopulator.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
  - llm/utility
  - llm/validation
---
# BookPhonemePopulator::MergeVariants
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookPhonemePopulator.cs`

## Summary
**Merges phoneme variant arrays into a trimmed, deduplicated, capacity-limited result.**

`MergeVariants` combines existing and incoming phoneme variants into a deduplicated, case-insensitive set, preserving insertion order semantics of the add sequence (current first, then incoming). Its local `AddRange` helper filters null/whitespace variants, trims entries, and stops once `MaxPhonemeVariantsPerWord` is reached. The method always seeds from `current`, then adds `incoming` only if capacity remains, and returns the merged set as an array.


#### [[BookPhonemePopulator.MergeVariants]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string[] MergeVariants(string[] current, string[] incoming)
```

**Calls ->**
- [[AddRange]]

**Called-by <-**
- [[BookPhonemePopulator.PopulateMissingAsync]]

