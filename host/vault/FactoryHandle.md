---
namespace: "Ams.Core.Processors"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs"
access_modifier: "private"
base_class: ~
interfaces:
  - "System.IDisposable"
member_count: 2
dependency_count: 2
pattern: ~
tags:
  - class
---

# FactoryHandle

> Class in `Ams.Core.Processors`

**Path**: `Projects/AMS/host/Ams.Core/Processors/AsrProcessor.cs`

**Implements**:
- IDisposable

## Dependencies
- [[FactoryKey]] (`key`)
- [[FactoryEntry]] (`entry`)

## Properties
- `Factory`: WhisperFactory
- `_key`: FactoryKey
- `_entry`: FactoryEntry?

## Members
- [[FactoryHandle..ctor]]
- [[FactoryHandle.Dispose]]

