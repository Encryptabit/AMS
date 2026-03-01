---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.cs"
access_modifier: "public"
complexity: 4
fan_in: 3
fan_out: 1
tags:
  - method
---
# AudioProcessor::EncodeWav
**Path**: `home/cari/repos/AMS/host/Ams.Core/Processors/AudioProcessor.cs`


#### [[AudioProcessor.EncodeWav]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public static void EncodeWav(string path, AudioBuffer buffer, AudioEncodeOptions options = null)
```

**Calls ->**
- [[FfEncoder.EncodeToCustomStream]]

**Called-by <-**
- [[AudioTreatmentService.TreatChapterCoreAsync]]
- [[PolishService.PersistCorrectedBuffer]]
- [[UndoService.SaveOriginalSegment]]

