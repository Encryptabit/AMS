---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Asr/AsrClient.cs"
access_modifier: "public"
complexity: 2
fan_in: 0
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/error-handling
---
# AsrClient::Dispose
**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrClient.cs`

## Summary
**Release the client’s `HttpClient` resources safely and mark the instance as disposed.**

`Dispose` implements idempotent teardown for `AsrClient` by checking `_disposed` before disposing the owned `_httpClient` and flipping the flag. It then calls `GC.SuppressFinalize(this)` to avoid finalization overhead (even though no finalizer is shown). The method ensures subsequent operations can detect disposal via the `_disposed` guard used elsewhere.


#### [[AsrClient.Dispose]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public void Dispose()
```

