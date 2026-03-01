---
namespace: "Ams.Core.Services.Integrations.FFmpeg"
project: "Ams.Core"
source_file: "Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfSession.cs"
access_modifier: "private"
complexity: 11
fan_in: 2
fan_out: 1
tags:
  - method
  - llm/data-access
  - llm/validation
  - llm/error-handling
---
# FfSession::HasNativeLibraries
**Path**: `Projects/AMS/host/Ams.Core/Services/Integrations/FFmpeg/FfSession.cs`

## Summary
**Determines whether a directory (or its `bin` child) appears to contain required FFmpeg native library files.**

`HasNativeLibraries` probes a directory for FFmpeg native binaries by first checking `Directory.Exists`, then scanning top-level files for names starting with required prefixes (`avcodec`, `avformat`, `avutil`, `avfilter`) using case-insensitive matching. If no direct match is found, it recursively checks only subdirectories named `bin`, supporting nested layouts. The method wraps filesystem enumeration in a broad `try/catch`; any I/O/access failure is treated as “not found” and returns `false`.


#### [[FfSession.HasNativeLibraries]]
##### What it does:
- _TODO: Plain-English walkthrough._

##### Improvements:
- _TODO: Suggested optimizations._

```csharp
private static bool HasNativeLibraries(string directory)
```

**Calls ->**
- [[FfSession.HasNativeLibraries]]

**Called-by <-**
- [[FfSession.HasNativeLibraries]]
- [[FfSession.TrySet]]

