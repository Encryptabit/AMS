---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfSession.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IDisposable"
member_count: 9
dependency_count: 0
pattern: ~
tags:
  - class
---

# FfSession

> Class in `Ams.Core.Services.Integrations.FFmpeg`

**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfSession.cs`

**Implements**:
- IDisposable

## Properties
- `FiltersAvailable`: bool
- `InitLock`: object
- `_initialized`: bool
- `_filtersChecked`: bool
- `_filtersAvailable`: bool
- `RootSearchSuffixes`: string[]

## Members
- [[FfSession.EnsureInitialized]]
- [[FfSession.EnsureFiltersAvailable]]
- [[FfSession.TrySetRootPath]]
- [[FfSession.TrySet]]
- [[FfSession.HasNativeLibraries]]
- [[FfSession.IsBindingException]]
- [[FfSession.EnsureFilterProbe]]
- [[FfSession.BuildFailureHint]]
- [[FfSession.Dispose]]

