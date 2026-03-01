---
namespace: "Ams.Core.Validation"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Validation/ScriptValidator.cs"
access_modifier: "private"
complexity: 6
fan_in: 1
fan_out: 2
tags:
  - method
---
# ScriptValidator::ExtractWordsFromAsrResponse
**Path**: `home/cari/repos/AMS/host/Ams.Core/Validation/ScriptValidator.cs`


#### [[ScriptValidator.ExtractWordsFromAsrResponse]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private List<ScriptValidator.WordAlignment> ExtractWordsFromAsrResponse(AsrResponse asrResponse)
```

**Calls ->**
- [[AsrResponse.GetWord]]
- [[TextNormalizer.Normalize]]

**Called-by <-**
- [[ScriptValidator.Validate]]

