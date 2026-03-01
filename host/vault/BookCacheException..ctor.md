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
**Constructs a `BookCacheException` with a supplied error message.**

This constructor is a direct domain-specific pass-through to `Exception` via `: base(message)`. It adds no additional fields or behavior, and exists to classify cache-related failures with a dedicated exception type. Message formatting/validation is left to callers and the base class.


#### [[BookCacheException..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BookCacheException(string message)
```

