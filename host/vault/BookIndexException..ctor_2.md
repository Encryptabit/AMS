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
# BookIndexException::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs`

## Summary
**Constructs a `BookIndexException` with both a message and an inner exception for chained error reporting.**

This overload initializes `BookIndexException` by delegating directly to `Exception` (`: base(message, innerException)`), preserving message text and causal exception chain. It introduces no additional fields, normalization, or validation. The constructor provides domain-specific wrapping for indexing failures while retaining underlying exception context.


#### [[BookIndexException..ctor_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BookIndexException(string message, Exception innerException)
```

