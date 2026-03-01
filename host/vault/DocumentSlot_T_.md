---
namespace: "Ams.Core.Runtime.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Common/DocumentSlot.cs"
access_modifier: "internal"
base_class: ~
interfaces: []
member_count: 8
dependency_count: 2
pattern: ~
tags:
  - class
---

# DocumentSlot

> Class in `Ams.Core.Runtime.Common`

**Path**: `Projects/AMS/host/Ams.Core/Runtime/Common/DocumentSlot.cs`

## Dependencies
- [[Ams.Core.Runtime.Common.DocumentSlotOptions_T__]] (`options`)
- [[IDocumentSlotAdapter]] (`adapter`)

## Properties
- `IsDirty`: bool
- `_loader`: Func<T?>
- `_saver`: Action<T>
- `_postLoadTransform`: Func<T?, T?>?
- `_writeThrough`: bool
- `_backingFileAccessor`: Func<FileInfo?>?
- `_adapter`: IDocumentSlotAdapter<T>?
- `_loaded`: bool
- `_dirty`: bool
- `_value`: T?

## Members
- [[DocumentSlot_T_..ctor_2]]
- [[DocumentSlot_T_..ctor]]
- [[DocumentSlot_T_.GetValue]]
- [[DocumentSlot_T_.SetValue]]
- [[DocumentSlot_T_.SetValue_2]]
- [[DocumentSlot_T_.Invalidate]]
- [[DocumentSlot_T_.GetBackingFile]]
- [[DocumentSlot_T_.Save]]

