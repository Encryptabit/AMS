---
namespace: "Ams.Workstation.Server.Controllers"
project: "Ams.Workstation.Server"
source_file: "Projects/AMS/host/Ams.Workstation.Server/Controllers/AudioController.cs"
access_modifier: "public"
base_class: "Microsoft.AspNetCore.Mvc.ControllerBase"
interfaces: []
member_count: 8
dependency_count: 2
pattern: "controller"
tags:
  - class
  - pattern/controller
---

# AudioController

> Class in `Ams.Workstation.Server.Controllers`

**Path**: `Projects/AMS/host/Ams.Workstation.Server/Controllers/AudioController.cs`

**Inherits from**: ControllerBase

## Dependencies
- [[BlazorWorkspace]] (`workspace`)
- [[PreviewBufferService]] (`previewBuffer`)

## Properties
- `_environment`: IWebHostEnvironment
- `_logger`: ILogger<AudioController>
- `_workspace`: BlazorWorkspace
- `_previewBuffer`: PreviewBufferService

## Members
- [[AudioController..ctor]]
- [[AudioController.GetAudio]]
- [[AudioController.GetChapterAudio]]
- [[AudioController.GetChapterRegionAudio]]
- [[AudioController.GetPreviewAudio]]
- [[AudioController.GetCorrectedChapterAudio]]
- [[AudioController.GetWaveformData]]
- [[AudioController.ServeAudioFile]]

