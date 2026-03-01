---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Asr/AsrClient.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
  - llm/utility
  - llm/validation
  - llm/error-handling
---
# AsrClient::ResolveTimeout
**Path**: `Projects/AMS/host/Ams.Core/Asr/AsrClient.cs`

## Summary
**Compute the ASR client request timeout from environment configuration with a safe default.**

`ResolveTimeout` derives the HTTP timeout from the `AMS_ASR_HTTP_TIMEOUT_SECONDS` environment variable. It parses the value with `int.TryParse` and accepts only positive seconds (`> 0`), returning `TimeSpan.FromSeconds(seconds)` when valid. If parsing fails or the value is non-positive, it falls back to `TimeSpan.FromMinutes(15)`.


#### [[AsrClient.ResolveTimeout]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static TimeSpan ResolveTimeout()
```

**Called-by <-**
- [[AsrClient..ctor]]

