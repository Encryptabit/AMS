---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.cs"
access_modifier: "public"
complexity: 13
fan_in: 1
fan_out: 0
tags:
  - method
---
# SilenceLogParser::Parse
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.cs`


#### [[SilenceLogParser.Parse]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static IReadOnlyList<SilenceInterval> Parse(IEnumerable<string> logs)
```

**Called-by <-**
- [[AudioProcessor.DetectSilence]]

