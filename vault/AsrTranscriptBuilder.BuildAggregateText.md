---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Asr/AsrTranscriptBuilder.cs"
access_modifier: "private"
complexity: 10
fan_in: 1
fan_out: 1
tags:
  - method
---
# AsrTranscriptBuilder::BuildAggregateText
**Path**: `home/cari/repos/AMS/host/Ams.Core/Asr/AsrTranscriptBuilder.cs`


#### [[AsrTranscriptBuilder.BuildAggregateText]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static string BuildAggregateText(AsrResponse response)
```

**Calls ->**
- [[AsrResponse.GetWord]]

**Called-by <-**
- [[AsrTranscriptBuilder.BuildSentences]]

