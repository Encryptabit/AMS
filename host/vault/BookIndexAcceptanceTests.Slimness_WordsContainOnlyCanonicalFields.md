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
# BookIndexAcceptanceTests::Slimness_WordsContainOnlyCanonicalFields
**Path**: `Projects/AMS/host/Ams.Tests/BookParsingTests.cs`

## Summary
**Ensure the generated slimness word records in the book index include only canonical fields.**

`Slimness_WordsContainOnlyCanonicalFields` is an async acceptance test that exercises the index pipeline by calling `BuildBookIndexAsync` and then `ParseBookAsync` on the produced artifact. It validates that the parsed word-level slimness payload contains only canonical field names, enforcing the schema contract at the boundary. Given complexity 2, the implementation is likely a tight Arrange/Act/Assert flow with one simple assertion path (e.g., a loop or predicate check over parsed fields).


#### [[BookIndexAcceptanceTests.Slimness_WordsContainOnlyCanonicalFields]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task Slimness_WordsContainOnlyCanonicalFields()
```

**Calls ->**
- [[DocumentProcessor.BuildBookIndexAsync]]
- [[DocumentProcessor.ParseBookAsync]]

