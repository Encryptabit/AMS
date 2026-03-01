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
  - llm/utility
---
# BookParseException::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/IBookServices.cs`

## Summary
**Constructs a `BookParseException` with a caller-provided message.**

This constructor is a pass-through exception initializer that delegates directly to `Exception` via `: base(message)`. It introduces no extra state, validation, or formatting logic beyond setting the exception message. The overload exists to provide a domain-specific exception type for parse failures.


#### [[BookParseException..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public BookParseException(string message)
```

