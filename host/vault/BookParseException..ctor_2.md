---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/error-handling
---
# BookParseException::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs`

## Summary
**Constructs a `BookParseException` that preserves an underlying exception as the inner cause.**

This overload initializes `BookParseException` with both a message and causal exception by delegating to `Exception` via `: base(message, innerException)`. It adds no custom fields or transformation logic, preserving the standard .NET exception chaining model. The constructor exists to wrap lower-level parse failures with domain-specific context.


#### [[BookParseException..ctor_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BookParseException(string message, Exception innerException)
```

