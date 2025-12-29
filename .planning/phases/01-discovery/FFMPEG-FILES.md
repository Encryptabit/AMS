# FFmpeg Integration - File Inventory

**Purpose:** Document all FFmpeg-related code files in the AMS codebase.

**Note:** FFmpeg integration uses `FFmpeg.AutoGen` NuGet package which provides managed P/Invoke wrappers for native FFmpeg libraries (libavformat, libavcodec, libavutil, libavfilter, libswresample). There are no direct `[DllImport]` declarations in AMS code - all native interop goes through FFmpeg.AutoGen's generated bindings.

## Folder Structure

```
host/Ams.Core/Services/Integrations/FFmpeg/
    FfDecoder.cs          # Audio decoding (probe & decode)
    FfEncoder.cs          # Audio encoding (WAV output)
    FfFilterGraph.cs      # Fluent filter graph builder
    FfFilterGraphRunner.cs # Filter graph execution engine
    FfLogCapture.cs       # FFmpeg log callback capture
    FfResampler.cs        # Placeholder (empty)
    FfSession.cs          # Global initialization & availability
    FfUtils.cs            # Error handling & utility functions
    FilterSpecs.cs        # Filter parameter record types
```

## File Inventory

| File | Category | Lines | Description |
|------|----------|-------|-------------|
| `FfDecoder.cs` | Core/Decode | ~530 | Audio probing and decoding with resampling support |
| `FfEncoder.cs` | Core/Encode | ~730 | WAV/PCM encoding with streaming support |
| `FfFilterGraph.cs` | Fluent API | ~620 | Fluent builder for libavfilter graphs |
| `FfFilterGraphRunner.cs` | Core/Filter | ~550 | Filter graph execution and frame management |
| `FfLogCapture.cs` | Utilities | ~67 | Thread-local FFmpeg log capture mechanism |
| `FfSession.cs` | Initialization | ~220 | FFmpeg initialization and availability checking |
| `FfUtils.cs` | Utilities | ~150 | Error formatting, channel layout, resampling math |
| `FfResampler.cs` | Placeholder | ~8 | Empty placeholder class |
| `FilterSpecs.cs` | Data Types | ~68 | Parameter record types for filter configuration |

## Consumer Files (Using FFmpeg)

| File | Category | FFmpeg Usage |
|------|----------|--------------|
| `Processors/AudioProcessor.cs` | Primary API | `FfDecoder`, `FfEncoder`, `FfFilterGraph`, `FfFilterGraphRunner` |
| `Processors/AudioProcessor.Analysis.cs` | Analysis | Extended audio analysis operations |
| `Processors/AsrProcessor.cs` | ASR | Uses `AudioProcessor.Decode/Resample` |
| `Runtime/Audio/AudioBufferManager.cs` | Buffer Mgmt | Uses `AudioProcessor.Decode` |
| `Audio/SentenceTimelineBuilder.cs` | Timeline | Uses `AudioProcessor` |
| `Services/AsrService.cs` | Service Layer | Uses `AudioProcessor` |

## CLI/Host Consumer Files

| File | Category | FFmpeg Usage |
|------|----------|--------------|
| `Ams.Cli/Commands/DspCommand.cs` | CLI | `AudioProcessor.Decode`, `FfFilterGraph` |
| `Ams.Cli/Commands/PipelineCommand.cs` | CLI | Indirect via processors |
| `Ams.Cli/Commands/ValidateCommand.cs` | CLI | Indirect via processors |
| `Ams.Web.Api/Program.cs` | Web | Initialization setup |

## Test Files

| File | Category | Description |
|------|----------|-------------|
| `Ams.Tests/WavIoTests.cs` | Unit Test | WAV decode/encode round-trip tests |
| `Ams.Tests/AudioProcessorFilterTests.cs` | Unit Test | Filter graph operation tests |

## External Assets

| Path | Type | Description |
|------|------|-------------|
| `host/Ams.Core/ExtTools/ffmpeg/bin/` | Native DLLs | FFmpeg shared libraries location |
| `analysis/ffmpeg-intergration.md` | Documentation | Design notes for FFmpeg integration |

## NuGet Dependencies

The FFmpeg integration relies on the **FFmpeg.AutoGen** NuGet package which provides:
- Managed wrappers for all FFmpeg library functions
- Pointer-based unsafe interop that mirrors native C API
- Platform-independent loading of native libraries
- All FFmpeg data structures as C# structs

## Code Statistics

- **Total FFmpeg integration files:** 9 source files
- **Total lines of FFmpeg code:** ~2,900 lines
- **Consumer files:** 10 files directly use FFmpeg APIs
- **Test files:** 2 test files
