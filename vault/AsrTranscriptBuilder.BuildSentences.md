---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Asr/AsrTranscriptBuilder.cs"
access_modifier: "public"
complexity: 6
fan_in: 1
fan_out: 1
tags:
  - method
---
# AsrTranscriptBuilder::BuildSentences
**Path**: `home/cari/repos/AMS/host/Ams.Core/Asr/AsrTranscriptBuilder.cs`


#### [[AsrTranscriptBuilder.BuildSentences]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyList<string> BuildSentences(AsrResponse response)
```

**Calls ->**
- [[AsrTranscriptBuilder.BuildAggregateText]]

**Called-by <-**
- [[AsrTranscriptBuilder.BuildCorpusText]]

