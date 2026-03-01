---
namespace: "Ams.Core.Runtime.Book"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs"
access_modifier: "public"
complexity: 3
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AudioBufferDescriptor::.ctor
**Path**: `Projects/AMS/host/Ams.Core/Runtime/Book/BookManager.cs`

## Summary
**Creates an audio buffer descriptor with required identity/path fields and optional decode/segment parameters.**

The constructor initializes an immutable `AudioBufferDescriptor` record from caller-supplied metadata and optional slicing/format hints. It enforces required fields by throwing `ArgumentNullException` when `bufferId` or `path` is null, then assigns nullable `SampleRate`, `Channels`, `Start`, and `Duration` directly. No normalization or range validation is applied to optional values.


#### [[AudioBufferDescriptor..ctor]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AudioBufferDescriptor(string bufferId, string path, int? sampleRate = null, int? channels = null, TimeSpan? start = null, TimeSpan? duration = null)
```

