---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/UndoService.cs"
access_modifier: "public"
complexity: 2
fan_in: 2
fan_out: 6
tags:
  - method
---
# UndoService::SaveOriginalSegment
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/UndoService.cs`


#### [[UndoService.SaveOriginalSegment]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.UndoService.SaveOriginalSegment(System.String,System.Int32,System.String,Ams.Core.Artifacts.AudioBuffer,System.Double,System.Double,System.Double)">
    <summary>
    Saves the original audio segment before a replacement is applied.
    Trims the segment from <paramref name="startSec"/> to <paramref name="endSec"/>,
    encodes as WAV, and creates an <see cref="T:Ams.Workstation.Server.Models.UndoRecord"/>.
    </summary>
    <param name="chapterStem">The chapter stem identifier.</param>
    <param name="sentenceId">The sentence being replaced.</param>
    <param name="replacementId">The ID of the StagedReplacement being applied.</param>
    <param name="originalBuffer">The full chapter audio buffer.</param>
    <param name="startSec">Start time of the segment to back up.</param>
    <param name="endSec">End time of the segment to back up.</param>
    <param name="replacementDurationSec">Duration of the replacement audio (for shift tracking).</param>
    <returns>The created UndoRecord.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public UndoRecord SaveOriginalSegment(string chapterStem, int sentenceId, string replacementId, AudioBuffer originalBuffer, double startSec, double endSec, double replacementDurationSec)
```

**Calls ->**
- [[AudioProcessor.EncodeWav]]
- [[AudioProcessor.Trim]]
- [[UndoService.EnsureLoaded]]
- [[UndoService.GetChapterUndoDir]]
- [[UndoService.GetNextVersion]]
- [[UndoService.SaveManifest]]

**Called-by <-**
- [[PolishService.ApplyReplacementAsync]]
- [[PolishService.ApplyRoomtoneOperationAsync]]

