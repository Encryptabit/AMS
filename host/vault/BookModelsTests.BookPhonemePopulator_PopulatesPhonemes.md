---
namespace: "Ams.Tests"
project: "Ams.Tests"
source_file: "Projects/AMS/host/Ams.Tests/BookParsingTests.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 3
tags:
  - method
---
# BookModelsTests::BookPhonemePopulator_PopulatesPhonemes
**Path**: `Projects/AMS/host/Ams.Tests/BookParsingTests.cs`


#### [[BookModelsTests.BookPhonemePopulator_PopulatesPhonemes]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task BookPhonemePopulator_PopulatesPhonemes()
```

**Calls ->**
- [[DocumentProcessor.BuildBookIndexAsync]]
- [[DocumentProcessor.ParseBookAsync]]
- [[DocumentProcessor.PopulateMissingPhonemesAsync]]

