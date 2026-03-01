---
namespace: "Ams.Workstation.Server.Controllers"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Controllers/AudioController.cs"
access_modifier: "public"
complexity: 10
fan_in: 0
fan_out: 4
tags:
  - method
---
# AudioController::GetChapterRegionAudio
**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Controllers/AudioController.cs`


#### [[AudioController.GetChapterRegionAudio]]
##### What it does:
<member name="M:Ams.Workstation.Server.Controllers.AudioController.GetChapterRegionAudio(System.String,System.Double,System.Double)">
    <summary>
    Serves a partial region of a chapter's audio by decoding only the requested time range.
    Uses the workspace to resolve the chapter's audio file path and decodes directly from disk
    with start/duration parameters for memory-efficient partial loading.
    </summary>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public IActionResult GetChapterRegionAudio(string chapterName, double start, double end)
```

**Calls ->**
- [[AudioBuffer.ToWavStream]]
- [[AudioProcessor.Decode]]
- [[AudioProcessor.Probe]]
- [[BlazorWorkspace.GetStemForChapter]]

