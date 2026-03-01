---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Asr/AsrClient.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 0
tags:
  - method
---
# AsrClient::IsHealthyAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Asr/AsrClient.cs`


#### [[AsrClient.IsHealthyAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default(CancellationToken))
```

**Called-by <-**
- [[GenerateTranscriptCommand.RunNemoAsync]]
- [[AsrProcessSupervisor.IsHealthyAsync]]

