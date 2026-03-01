---
namespace: "Ams.Tests"
project: "Ams.Tests"
source_file: "Projects/AMS/host/Ams.Tests/BookParsingTests.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/di
---
# StubPronunciationProvider::.ctor
**Path**: `Projects/AMS/host/Ams.Tests/BookParsingTests.cs`

## Summary
**Initialize the test stub pronunciation provider with a predefined token-to-pronunciations map.**

This constructor on `StubPronunciationProvider` accepts an `IReadOnlyDictionary<string, string[]>` and stores it as the stub’s backing pronunciation lookup used by tests. Given complexity 1, it is a linear initialization step (constructor injection/assignment) with no branching, async flow, or internal processing.


#### [[StubPronunciationProvider..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public StubPronunciationProvider(IReadOnlyDictionary<string, string[]> map)
```

