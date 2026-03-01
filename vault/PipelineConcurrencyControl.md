---
namespace: "Ams.Core.Application.Pipeline"
project: "Ams.Core"
source_file: "home/cari/repos/AMS/host/Ams.Core/Application/Pipeline/PipelineConcurrencyControl.cs"
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

# PipelineConcurrencyControl

> Class in `Ams.Core.Application.Pipeline`

**Path**: `home/cari/repos/AMS/host/Ams.Core/Application/Pipeline/PipelineConcurrencyControl.cs`

**Implements**:
- IDisposable

## Properties
- `BookIndexSemaphore`: SemaphoreSlim
- `AsrSemaphore`: SemaphoreSlim
- `MfaSemaphore`: SemaphoreSlim
- `MfaDegree`: int
- `_bookIndexForceClaimed`: int
- `_mfaWorkspaceQueue`: ConcurrentQueue<string>
- `_mfaWorkspaces`: List<string>
- `_mfaWorkspaceSet`: HashSet<string>

## Members
- [[PipelineConcurrencyControl..ctor]]
- [[PipelineConcurrencyControl.CreateSingle]]
- [[PipelineConcurrencyControl.Create]]
- [[PipelineConcurrencyControl.CreateShared]]
- [[PipelineConcurrencyControl.TryClaimBookIndexForce]]
- [[PipelineConcurrencyControl.RentMfaWorkspace]]
- [[PipelineConcurrencyControl.ReturnMfaWorkspace]]
- [[PipelineConcurrencyControl.Dispose]]
- [[PipelineConcurrencyControl.ResolveWorkspaceRoots]]

