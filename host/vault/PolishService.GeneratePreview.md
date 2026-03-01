---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Services/PolishService.cs"
access_modifier: "public"
complexity: 1
fan_in: 0
fan_out: 7
tags:
  - method
---
# PolishService::GeneratePreview
**Path**: `Projects/AMS/host/Ams.Workstation.Server/Services/PolishService.cs`


#### [[PolishService.GeneratePreview]]
##### What it does:
<member name="M:Ams.Workstation.Server.Services.PolishService.GeneratePreview(System.String)">
    <summary>
    Generates a preview by splicing the pickup into the chapter buffer in memory.
    The result is cached in <see cref="T:Ams.Workstation.Server.Services.PreviewBufferService"/> for streaming via the API
    but is NOT written to disk.
    </summary>
    <param name="replacementId">ID of the staged replacement to preview.</param>
    <returns>The spliced preview buffer.</returns>
</member>

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
public AudioBuffer GeneratePreview(string replacementId)
```

**Calls ->**
- [[AudioSpliceService.ReplaceSegment]]
- [[AudioProcessor.Decode]]
- [[PolishService.FindStagedItem]]
- [[PolishService.GetActiveChapterHandleOrThrow]]
- [[PolishService.GetChapterBuffer]]
- [[PolishService.TrimPickupForReplacement]]
- [[PreviewBufferService.Set]]

