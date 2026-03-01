---
namespace: "Ams.Core.Validation"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Validation/ScriptValidator.cs"
access_modifier: "private"
complexity: 1
fan_in: 1
fan_out: 2
tags:
  - method
---
# ScriptValidator::ExtractWordsFromScript
**Path**: `home/cari/repos/AMS/host/Ams.Core/Validation/ScriptValidator.cs`


#### [[ScriptValidator.ExtractWordsFromScript]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private List<string> ExtractWordsFromScript(string scriptText)
```

**Calls ->**
- [[TextNormalizer.Normalize]]
- [[TextNormalizer.TokenizeWords]]

**Called-by <-**
- [[ScriptValidator.Validate]]

