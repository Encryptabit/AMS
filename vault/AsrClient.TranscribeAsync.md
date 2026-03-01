---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Asr/AsrClient.cs"
access_modifier: "public"
complexity: 6
fan_in: 1
fan_out: 0
tags:
  - method
---
# AsrClient::TranscribeAsync
**Path**: `home/cari/repos/AMS/host/Ams.Core/Asr/AsrClient.cs`


#### [[AsrClient.TranscribeAsync]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public Task<AsrResponse> TranscribeAsync(string audioPath, string model = null, string language = "en", CancellationToken cancellationToken = default(CancellationToken))
```

**Called-by <-**
- [[GenerateTranscriptCommand.RunNemoAsync]]

