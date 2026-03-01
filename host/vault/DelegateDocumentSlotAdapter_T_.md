---
namespace: "Ams.Core.Runtime.Common"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Runtime/Common/DelegateDocumentSlotAdapter.cs"
access_modifier: "internal"
base_class: ~
interfaces:
  - "Ams.Core.Runtime.Common.IDocumentSlotAdapter<T>"
member_count: 4
dependency_count: 0
pattern: ~
tags:
  - class
---

# DelegateDocumentSlotAdapter

> Class in `Ams.Core.Runtime.Common`

**Path**: `Projects/AMS/host/Ams.Core/Runtime/Common/DelegateDocumentSlotAdapter.cs`

**Implements**:
- [[IDocumentSlotAdapter]]

## Properties
- `_loader`: Func<T?>
- `_saver`: Action<T>
- `_backingFileAccessor`: Func<FileInfo?>?

## Members
- [[DelegateDocumentSlotAdapter_T_..ctor]]
- [[DelegateDocumentSlotAdapter_T_.Load]]
- [[DelegateDocumentSlotAdapter_T_.Save]]
- [[DelegateDocumentSlotAdapter_T_.GetBackingFile]]

