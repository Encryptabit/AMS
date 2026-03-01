---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs"
access_modifier: "private"
complexity: 4
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# FfEncoder::UnpinChannels
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs`

## Summary
**Unpins previously pinned managed audio channel arrays to restore normal GC movement.**

This cleanup helper releases pinned channel buffers after FFmpeg interop use. It short-circuits when the array is empty, then iterates all handles and calls `Free()` only for entries where `IsAllocated` is true. The guarded free pattern avoids invalid-handle operations and makes teardown idempotent-safe.


#### [[FfEncoder.UnpinChannels]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static void UnpinChannels(GCHandle[] handles)
```

**Called-by <-**
- [[FfEncoder.Encode]]

