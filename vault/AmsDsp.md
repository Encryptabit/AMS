---
namespace: "Ams.Dsp.Native"
project: "Ams.Dsp.Native"
source_file: "home/cari/repos/AMS/host/Ams.Dsp.Native/AmsDsp.cs"
access_modifier: "public"
base_class: ~
interfaces:
  - "System.IDisposable"
member_count: 12
dependency_count: 0
pattern: ~
tags:
  - class
---

# AmsDsp

> Class in `Ams.Dsp.Native`

**Path**: `home/cari/repos/AMS/host/Ams.Dsp.Native/AmsDsp.cs`

**Implements**:
- IDisposable

## Properties
- `Channels`: int
- `MaxBlock`: int
- `SampleRate`: float
- `LatencySamples`: uint
- `ExpectedAbiMajor`: int
- `ExpectedAbiMinor`: int
- `_sync`: object
- `_initialized`: bool
- `_channels`: uint
- `_maxBlock`: uint
- `_sampleRate`: float

## Members
- [[AmsDsp..ctor]]
- [[AmsDsp.Create]]
- [[AmsDsp.Reset]]
- [[AmsDsp.SetParameter]]
- [[AmsDsp.ProcessBlock]]
- [[AmsDsp.ProcessLong]]
- [[AmsDsp.SaveState]]
- [[AmsDsp.LoadState]]
- [[AmsDsp.EnsureInit]]
- [[AmsDsp.ValidatePlanarBuffers]]
- [[AmsDsp.Dispose]]
- [[AmsDsp.Finalize]]

