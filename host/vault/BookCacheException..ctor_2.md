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
# BookCacheException::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs`

## Summary
**Constructs a `BookCacheException` that includes both an error message and inner exception context.**

This overload initializes `BookCacheException` by delegating to the base exception constructor (`: base(message, innerException)`), preserving both human-readable context and underlying cause. It introduces no custom fields or transformation logic. The constructor exists for domain-specific cache error wrapping with standard .NET exception chaining.


#### [[BookCacheException..ctor_2]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BookCacheException(string message, Exception innerException)
```

