---
namespace: "Ams.Tests"
project: "Ams.Tests"
source_file: "Projects/AMS/host/Ams.Tests/BookParsingTests.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 1
tags:
  - method
  - llm/async
  - llm/validation
  - llm/error-handling
---
# BookParsingTests::Parser_Unsupported_Throws
**Path**: `Projects/AMS/host/Ams.Tests/BookParsingTests.cs`

## Summary
**Verify that `ParseBookAsync` rejects unsupported parsing scenarios by throwing rather than succeeding.**

Parser_Unsupported_Throws is a Task-based test in `Ams.Tests.BookParsingTests` that exercises the failure path of `ParseBookAsync` for unsupported parser/input conditions. With very low branching (complexity 2), the implementation is focused on invoking `ParseBookAsync` and asserting that the call fails through the expected exception path.


#### [[BookParsingTests.Parser_Unsupported_Throws]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task Parser_Unsupported_Throws()
```

**Calls ->**
- [[DocumentProcessor.ParseBookAsync]]

