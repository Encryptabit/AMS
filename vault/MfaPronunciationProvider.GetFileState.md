---
namespace: "Ams.Core.Application.Mfa"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs"
access_modifier: "private"
complexity: 3
fan_in: 1
fan_out: 0
tags:
  - method
---
# MfaPronunciationProvider::GetFileState
**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Mfa/MfaPronunciationProvider.cs`


#### [[MfaPronunciationProvider.GetFileState]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static (bool Exists, long SizeBytes) GetFileState(string path)
```

**Called-by <-**
- [[MfaPronunciationProvider.RunG2pWithProgressAsync]]

