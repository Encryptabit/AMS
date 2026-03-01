---
namespace: "Ams.Core.Processors.Alignment.Tx"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs"
access_modifier: "public"
complexity: 3
fan_in: 1
fan_out: 1
tags:
  - method
---
# PhonemeComparer::Equals
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/Alignment/Tx/TranscriptAligner.cs`


#### [[PhonemeComparer.Equals]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static bool Equals(string a, string b)
```

**Calls ->**
- [[PhonemeComparer.Normalize]]

**Called-by <-**
- [[TranscriptAligner.HasExactPhonemeMatch]]

