---
namespace: "Ams.Tests"
project: "Ams.Tests"
source_file: "Projects/AMS/host/Ams.Tests/BookParsingTests.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 2
tags:
  - method
  - llm/async
  - llm/validation
---
# BookIndexAcceptanceTests::NoNormalization_WordsPreserveExactText
**Path**: `Projects/AMS/host/Ams.Tests/BookParsingTests.cs`

## Summary
**Verify that the book-indexing pipeline keeps word text verbatim when no normalization is applied.**

This asynchronous acceptance test in `Ams.Tests.BookIndexAcceptanceTests` returns `Task` and has low branching complexity (2). It drives the parse-and-index flow by calling `ParseBookAsync` and `BuildBookIndexAsync`, then asserts that indexed words preserve the exact original text when normalization is disabled. The behavior under test is strict text preservation rather than normalized token matching.


#### [[BookIndexAcceptanceTests.NoNormalization_WordsPreserveExactText]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task NoNormalization_WordsPreserveExactText()
```

**Calls ->**
- [[DocumentProcessor.BuildBookIndexAsync]]
- [[DocumentProcessor.ParseBookAsync]]

