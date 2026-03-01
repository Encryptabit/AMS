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
**Constructs a `BookIndexException` with a supplied error message.**

This constructor is a direct pass-through to the base `Exception` constructor (`: base(message)`) for a domain-specific indexing exception type. It does not add custom state, validation, or formatting logic. Its purpose is to provide typed error classification for book indexing failures.


#### [[BookIndexException..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BookIndexException(string message)
```

