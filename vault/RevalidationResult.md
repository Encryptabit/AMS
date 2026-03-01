---
namespace: "Ams.Workstation.Server.Services"
project: "Ams.Workstation.Server"
source_file: "home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PolishVerificationService.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IEquatable<Ams.Workstation.Server.Services.RevalidationResult>"
member_count: 1
dependency_count: 0
pattern: ~
tags:
  - class
---

# RevalidationResult

> Record in `Ams.Workstation.Server.Services`

**Path**: `home/cari/repos/AMS/host/Ams.Workstation.Server/Services/PolishVerificationService.cs`

**Implements**:
- IEquatable

## Properties
- `RecognizedText`: string
- `ExpectedText`: string
- `Similarity`: double
- `Passed`: bool
- `Tokens`: IReadOnlyList<AsrToken>

## Members

