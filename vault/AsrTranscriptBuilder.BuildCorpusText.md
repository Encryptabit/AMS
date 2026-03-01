---
namespace: "Ams.Core.Asr"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Asr/AsrTranscriptBuilder.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 1
tags:
  - method
---
# AsrTranscriptBuilder::BuildCorpusText
**Path**: `home/cari/repos/AMS/host/Ams.Core/Asr/AsrTranscriptBuilder.cs`


#### [[AsrTranscriptBuilder.BuildCorpusText]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static string BuildCorpusText(AsrResponse response)
```

**Calls ->**
- [[AsrTranscriptBuilder.BuildSentences]]

**Called-by <-**
- [[GenerateTranscriptCommand.PersistResponse]]
- [[ChapterManager.CreateContext]]

