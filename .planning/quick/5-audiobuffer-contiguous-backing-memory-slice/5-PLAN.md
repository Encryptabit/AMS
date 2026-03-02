---
phase: quick-5
plan: 01
type: execute
wave: 1
depends_on: []
files_modified:
  - host/Ams.Core/Artifacts/AudioBuffer.cs
  - host/Ams.Core/Audio/AudioDefaults.cs
  - host/Ams.Core/Processors/AsrProcessor.cs
  - host/Ams.Core/Audio/FeatureExtraction.cs
  - host/Ams.Core/Audio/AsrAudioPreparer.cs
  - host/Ams.Core/Audio/AudioSpliceService.cs
  - host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs
  - host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs
  - host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs
  - host/Ams.Core/Processors/AudioProcessor.Analysis.cs
  - host/Ams.Cli/Commands/PipelineCommand.cs
  - host/Ams.Workstation.Server/Controllers/AudioController.cs
  - host/Ams.Tests/WavIoTests.cs
  - host/Ams.Tests/AudioProcessorFilterTests.cs
autonomous: true
requirements: [AUDIO-BUFFER-REFACTOR, AUDIO-DEFAULTS]

must_haves:
  truths:
    - "AudioBuffer uses a single contiguous float[] backing store per channel instead of separate float[] arrays"
    - "AudioBuffer.Planar exposes ReadOnlyMemory<float> views (not raw float[]) per channel"
    - "AudioBuffer.Slice(startSample, length) returns a zero-copy view sharing the parent backing store"
    - "Sliced buffers produce correct WAV output via ToWavStream"
    - "FfEncoder GCHandle pinning works correctly with Memory<float>-backed buffers"
    - "AudioDefaults provides app-wide shared silence detection constants"
    - "GenerateRoomtoneFill guards against zero-length roomtone source to prevent infinite loop"
    - "All existing tests pass after migration"
  artifacts:
    - path: "host/Ams.Core/Artifacts/AudioBuffer.cs"
      provides: "Contiguous backing store, Memory<float> channel views, Slice method"
      contains: "ReadOnlyMemory"
    - path: "host/Ams.Core/Audio/AudioDefaults.cs"
      provides: "Shared silence detection constants"
      contains: "SilenceThresholdDb"
  key_links:
    - from: "AudioBuffer.Slice"
      to: "AudioBuffer constructor"
      via: "internal constructor accepting backing array + offset + length"
      pattern: "Slice.*offset.*length"
---

<objective>
Refactor AudioBuffer from float[][] to contiguous backing store with Memory<float> channel views, enabling zero-copy slicing. Also introduce shared AudioDefaults for app-wide silence detection constants.

Purpose: Vault-recommended infrastructure improvement. Enables zero-copy chunk extraction for ASR pre-chunking (quick-6) without per-chunk allocation or FFmpeg round-trips. The Memory<float> API also improves safety by preventing external mutation of buffer internals.

Output: Refactored AudioBuffer with Slice(), updated all 14 consumer files, new AudioDefaults static class, all tests passing.
</objective>

<execution_context>
@/home/cari/.claude/get-shit-done/workflows/execute-plan.md
@/home/cari/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@host/Ams.Core/Artifacts/AudioBuffer.cs
@host/Ams.Core/Processors/AsrProcessor.cs
@host/Ams.Core/Audio/FeatureExtraction.cs
@host/Ams.Core/Audio/AsrAudioPreparer.cs
@host/Ams.Core/Audio/AudioSpliceService.cs
@host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs
@host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs
@host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs
@host/Ams.Core/Processors/AudioProcessor.Analysis.cs
@host/Ams.Cli/Commands/PipelineCommand.cs
@host/Ams.Workstation.Server/Controllers/AudioController.cs
@host/Ams.Core/Audio/TreatmentOptions.cs
@host/Ams.Core/Audio/SpliceBoundaryService.cs

<interfaces>
Current AudioBuffer (to be refactored):
```csharp
public sealed class AudioBuffer
{
    public int Channels { get; }
    public int SampleRate { get; }
    public int Length { get; }
    public float[][] Planar { get; }     // <-- replace with Memory<float>[]
    public AudioBufferMetadata Metadata { get; private set; }

    public AudioBuffer(int channels, int sampleRate, int length, AudioBufferMetadata? metadata = null)
    {
        // Allocates float[channels][] with float[length] each
    }
}
```

Vault suggestions driving this refactor:
- AudioBuffer..ctor: "Reduce allocations by using a single contiguous float[] backing store with channel slices (Span/offset math)"
- AudioBuffer class: "Expose Planar as read-only views (ReadOnlyMemory<float>/IReadOnlyList<float[]>) and provide controlled mutation APIs"
- AudioBuffer.ToWavStream: "Add WriteWavTo(Stream) to avoid allocating MemoryStream"

Consumer access patterns:
- WRITES: Array.Copy, direct index assignment (AsrProcessor, AsrAudioPreparer, AudioSpliceService, FfDecoder, FfFilterGraphRunner)
- READS: Index access in loops, channel references (FeatureExtraction, AudioProcessor.Analysis, PipelineCommand, AudioController)
- PINNING: GCHandle.Alloc on Planar[ch] for FFmpeg unmanaged interop (FfEncoder — SAFETY CRITICAL)
- LINQ: .Take(), .Skip(), .Max() on channel arrays (tests only)

Existing silence defaults:
- SilenceDetectOptions default: NoiseDb = -50.0, MinimumDuration = 500ms
- TreatmentOptions default: SilenceThresholdDb = -55.0, MinimumSilenceDuration = 0.05
- SpliceBoundaryOptions default: SilenceThresholdDb = -55.0, MinSilenceDuration = 50ms
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task 1: Refactor AudioBuffer internals to contiguous backing + Memory views</name>
  <files>host/Ams.Core/Artifacts/AudioBuffer.cs</files>
  <action>
**1a. Change internal storage from `float[][]` to contiguous backing.**

Replace the current constructor that allocates separate `float[length]` per channel with:
- A single `float[]` backing array of size `channels * length`
- Channel access via `Memory<float>` slices at offsets `ch * length`

For sliced buffers (created by `Slice()`), the backing array is shared — offset and length differ.

**1b. New internal state:**

```csharp
public sealed class AudioBuffer
{
    private readonly float[] _backing;
    private readonly int _offset;  // sample offset into backing per-channel stride

    public int Channels { get; }
    public int SampleRate { get; }
    public int Length { get; }
    public AudioBufferMetadata Metadata { get; private set; }

    // Public API: read-only memory views per channel
    public ReadOnlyMemory<float> GetChannel(int channel)
        => new(_backing, channel * _strideLength + _offset, Length);

    // Writable span for producers (internal to avoid external mutation)
    internal Span<float> GetChannelSpan(int channel)
        => _backing.AsSpan(channel * _strideLength + _offset, Length);

    // For backward compat during migration, expose indexer
    // Consumers that need float[] can call .ToArray() explicitly
    public float this[int channel, int sample]
    {
        get => _backing[channel * _strideLength + _offset + sample];
        internal set => _backing[channel * _strideLength + _offset + sample] = value;
    }

    private readonly int _strideLength; // original backing length per channel (may differ from Length for slices)
}
```

**1c. Constructors:**

Primary constructor (new buffer with owned backing):
```csharp
public AudioBuffer(int channels, int sampleRate, int length, AudioBufferMetadata? metadata = null)
{
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(channels);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
    ArgumentOutOfRangeException.ThrowIfNegative(length);

    Channels = channels;
    SampleRate = sampleRate;
    Length = length;
    _strideLength = length;
    _offset = 0;
    _backing = length > 0 ? new float[channels * length] : Array.Empty<float>();
    Metadata = metadata ?? AudioBufferMetadata.CreateDefault(sampleRate, channels);
}
```

Internal slice constructor (shares parent backing):
```csharp
internal AudioBuffer(float[] backing, int channels, int sampleRate,
    int strideLength, int offset, int length, AudioBufferMetadata? metadata)
{
    _backing = backing;
    Channels = channels;
    SampleRate = sampleRate;
    _strideLength = strideLength;
    _offset = offset;
    Length = length;
    Metadata = metadata ?? AudioBufferMetadata.CreateDefault(sampleRate, channels);
}
```

**1d. Slice method:**

```csharp
public AudioBuffer Slice(int startSample, int length)
{
    ArgumentOutOfRangeException.ThrowIfNegative(startSample);
    ArgumentOutOfRangeException.ThrowIfNegative(length);
    if (startSample + length > Length)
        throw new ArgumentOutOfRangeException(nameof(length),
            $"Slice [{startSample}..{startSample + length}) exceeds buffer length {Length}");

    return new AudioBuffer(_backing, Channels, SampleRate,
        _strideLength, _offset + startSample, length, Metadata);
}

public AudioBuffer Slice(TimeSpan start, TimeSpan end)
{
    var startSample = (int)(start.TotalSeconds * SampleRate);
    var endSample = Math.Min((int)(end.TotalSeconds * SampleRate), Length);
    return Slice(startSample, endSample - startSample);
}
```

**1e. Update Concat to use new internals.**

Replace `Array.Copy(buf.Planar[ch], ...)` with span-based copies using `GetChannel`/`GetChannelSpan`.

**1f. Remove the `Planar` property.**

Replace `public float[][] Planar { get; }` with `GetChannel(int)` and `GetChannelSpan(int)`. If a temporary compatibility shim is needed during migration, add:

```csharp
[Obsolete("Use GetChannel(ch) or GetChannelSpan(ch) instead")]
public float[] GetChannelArray(int channel) => GetChannel(channel).ToArray();
```

But prefer updating consumers directly — there are only 14 files.
  </action>
  <verify>
    <automated>cd /home/cari/repos/AMS && dotnet build host/Ams.Core/Ams.Core.csproj --no-restore 2>&amp;1 | tail -10</automated>
  </verify>
  <done>
    - AudioBuffer uses contiguous float[] backing with channel strides
    - GetChannel(ch) returns ReadOnlyMemory&lt;float&gt;
    - GetChannelSpan(ch) returns writable Span&lt;float&gt; (internal)
    - Slice(startSample, length) returns zero-copy view
    - Slice(TimeSpan, TimeSpan) time-based overload
    - Constructor validates channels > 0, sampleRate > 0, length >= 0
    - Concat updated to use new API
    - Ams.Core compiles (consumers will break — fixed in Task 2)
  </done>
</task>

<task type="auto">
  <name>Task 2: Migrate all consumer files from Planar to GetChannel/GetChannelSpan</name>
  <files>
    host/Ams.Core/Processors/AsrProcessor.cs
    host/Ams.Core/Audio/FeatureExtraction.cs
    host/Ams.Core/Audio/AsrAudioPreparer.cs
    host/Ams.Core/Audio/AudioSpliceService.cs
    host/Ams.Core/Services/Integrations/FFmpeg/FfDecoder.cs
    host/Ams.Core/Services/Integrations/FFmpeg/FfEncoder.cs
    host/Ams.Core/Services/Integrations/FFmpeg/FfFilterGraphRunner.cs
    host/Ams.Core/Processors/AudioProcessor.Analysis.cs
    host/Ams.Cli/Commands/PipelineCommand.cs
    host/Ams.Workstation.Server/Controllers/AudioController.cs
    host/Ams.Tests/WavIoTests.cs
    host/Ams.Tests/AudioProcessorFilterTests.cs
  </files>
  <action>
Migrate each consumer from `buffer.Planar[ch]` / `buffer.Planar[ch][i]` to the new API. Pattern by access type:

**WRITES (populate buffers) — use `GetChannelSpan(ch)`:**
- `AsrProcessor.cs:136` — `Array.Copy(samples, buffer.Planar[0], ...)` → `samples.AsSpan().CopyTo(buffer.GetChannelSpan(0))`
- `AsrAudioPreparer.cs:137` — `mono.Planar[0][i] = ...` → `mono.GetChannelSpan(0)[i] = ...`
- `AudioSpliceService.cs:97-103` — `Array.Copy(src, 0, dst, cursor, toCopy)` → span slice copy
- `AudioSpliceService.cs:GenerateRoomtoneFill` — Add zero-length source guard: if `roomtone.Length == 0` and `targetDurationSec > 0`, throw `ArgumentException("Roomtone source buffer is empty")` to prevent infinite loop where `toCopy == 0` causes `while (cursor < targetSamples)` to never progress. Throwing is correct here because a zero-length roomtone is a caller bug, not a recoverable condition.
- `FfDecoder.cs:354` — `channelSamples[ch].CopyTo(buffer.Planar[ch], 0)` → copy to `GetChannelSpan(ch)`
- `FfFilterGraphRunner.cs:517` — `_channels[ch].CopyTo(buffer.Planar[ch], 0)` → copy to `GetChannelSpan(ch)`
- Test files — direct assignment `buffer.Planar[0][i] = value` → `buffer.GetChannelSpan(0)[i] = value`

**READS (loop access) — use `GetChannel(ch).Span`:**
- `AsrProcessor.cs:616,626` — channel reads for downmix → `var ch = buffer.GetChannel(ch).Span`
- `FeatureExtraction.cs:441,449` — return/loop channel → `buffer.GetChannel(0).Span` or `.ToArray()` if return type requires it
- `AsrAudioPreparer.cs:134` — downmix loop → span indexing
- `AudioProcessor.Analysis.cs:256` — RMS calc → `buffer.GetChannel(ch).Span`
- `AudioController.cs:329` — sample access → `buffer.GetChannel(ch).Span[s]`
- `PipelineCommand.cs:2535,2542,2793` — LUFS/downmix → span access

**PINNING (FfEncoder — SAFETY CRITICAL):**
- `FfEncoder.cs:341-342` — `GCHandle.Alloc(buffer.Planar[ch], GCHandleType.Pinned)` — this pins the raw array.
  With contiguous backing, pin the single `_backing` array once (expose via `internal float[] GetBackingArray()`), then compute channel offsets. Or pin per-channel Memory via `MemoryMarshal.TryGetArray()` → pin the underlying array.

  Safest approach: expose an `internal` method that returns the pinnable reference:
  ```csharp
  internal float[] GetBackingArray() => _backing;
  internal int GetChannelOffset(int channel) => channel * _strideLength + _offset;
  ```
  FfEncoder pins `_backing` once, then computes per-channel pointers from offset.

**CHANNEL COUNT CHECK:**
- `FeatureExtraction.cs:434` — `audio.Planar.Length == 0` → `audio.Channels == 0` (already a property)

**LINQ in tests:**
- `AudioProcessorFilterTests.cs:79,84` — `.Planar[0].Take(n)` → `buffer.GetChannel(0).ToArray().Take(n)` or convert to span-based assertions

**Vault improvements deferred** — AsrProcessor/FeatureExtraction optimizations (pre-sizing, AggregateTokens rewrite, AddRange, fast-path, consolidation) are out-of-scope for quick-5. Track separately.
  </action>
  <verify>
    <automated>cd /home/cari/repos/AMS && dotnet build host/Ams.sln --no-restore 2>&amp;1 | tail -20</automated>
  </verify>
  <done>
    - All 14 consumer files migrated from Planar to GetChannel/GetChannelSpan
    - FfEncoder pinning works correctly with contiguous backing
    - No remaining references to .Planar property
    - Full solution builds without errors
  </done>
</task>

<task type="auto">
  <name>Task 3: Add AudioDefaults and update tests</name>
  <files>
    host/Ams.Core/Audio/AudioDefaults.cs
    host/Ams.Tests/WavIoTests.cs
    host/Ams.Tests/AudioProcessorFilterTests.cs
    host/Ams.Tests/AudioBufferSliceTests.cs
  </files>
  <action>
**3a. Create AudioDefaults static class.**

Location: `host/Ams.Core/Audio/AudioDefaults.cs`

```csharp
namespace Ams.Core.Audio;

public static class AudioDefaults
{
    public const double SilenceThresholdDb = -55.0;
    public static readonly TimeSpan MinimumSilenceDuration = TimeSpan.FromMilliseconds(200);
}
```

These values match the existing TreatmentOptions/SpliceBoundaryOptions defaults (NoiseDb -55, bump MinDuration from 50ms to 200ms per user request). The 200ms bump prevents chunk boundaries from landing on micro-pauses that are too brief for clean ASR segmentation — the prior 50ms matched FFmpeg silence-detect sensitivity but was too aggressive for pre-chunking use cases. Existing option classes can be retrofitted to reference these later.

**3b. Add AudioBuffer slice tests.**

Create `host/Ams.Tests/AudioBufferSliceTests.cs`:

1. **Slice_SharesBacking** — Create buffer, write data, slice, verify slice reads same data without copy
2. **Slice_DoesNotAffectParent** — Verify parent Length unchanged after slicing
3. **Slice_OutOfBounds_Throws** — startSample + length > Length throws ArgumentOutOfRangeException
4. **Slice_TimeOverload** — TimeSpan-based slice produces correct sample boundaries
5. **Slice_ToWavStream_ProducesValidWav** — Sliced buffer encodes to valid WAV with correct sample count
6. **Slice_GetChannel_ReturnsCorrectRange** — GetChannel on slice returns only the sliced samples
7. **GetChannelSpan_WriteThrough** — Writing to GetChannelSpan on slice is visible in parent
8. **Contiguous_MultiChannel** — 2-channel buffer, verify channels are correctly strided in backing array

**3c. Verify existing tests pass after migration.**

Run full test suite — WavIoTests, AudioProcessorFilterTests, and all others must pass with the new AudioBuffer internals.
  </action>
  <verify>
    <automated>cd /home/cari/repos/AMS && dotnet test host/Ams.Tests/Ams.Tests.csproj --no-restore -v minimal 2>&amp;1 | tail -20</automated>
  </verify>
  <done>
    - AudioDefaults class with shared silence constants
    - 8 new AudioBuffer slice tests passing
    - All existing tests pass with new AudioBuffer internals
    - No regressions
  </done>
</task>

</tasks>

<verification>
```bash
# Full solution build
cd /home/cari/repos/AMS && dotnet build host/Ams.sln --no-restore

# Full test suite
cd /home/cari/repos/AMS && dotnet test host/Ams.Tests/Ams.Tests.csproj --no-restore -v minimal

# Verify no remaining Planar references (should be zero in non-test code)
grep -rn "\.Planar" host/Ams.Core/ host/Ams.Cli/ host/Ams.Workstation.Server/ --include="*.cs" | grep -v "Obsolete" | grep -v "//"

# Verify Slice method exists
grep -n "public AudioBuffer Slice" host/Ams.Core/Artifacts/AudioBuffer.cs

# Verify AudioDefaults exists
grep -n "SilenceThresholdDb" host/Ams.Core/Audio/AudioDefaults.cs
```
</verification>

<success_criteria>
- AudioBuffer uses contiguous float[] backing store (vault suggestion implemented)
- Planar replaced with ReadOnlyMemory<float> views (vault suggestion implemented)
- Slice() returns zero-copy view sharing parent backing
- FfEncoder pinning works correctly (safety critical)
- AudioDefaults provides shared silence constants (-55dB, 200ms)
- All 14 consumer files migrated
- All existing tests pass + new slice tests pass
</success_criteria>

<output>
After completion, create `.planning/quick/5-audiobuffer-contiguous-backing-memory-slice/5-SUMMARY.md`
</output>
