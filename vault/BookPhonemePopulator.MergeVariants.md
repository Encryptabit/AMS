---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookPhonemePopulator.cs"
access_modifier: "private"
complexity: 2
fan_in: 1
fan_out: 1
tags:
  - method
---
# BookPhonemePopulator::MergeVariants
**Path**: `home/cari/repos/AMS/host/Ams.Core/Runtime/Book/BookPhonemePopulator.cs`


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

