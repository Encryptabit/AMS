# FS04: Audio DSP, QC, And FFmpeg Integration

Last updated: 2026-05-17

Reader: an engineer changing audio decode, encode, trim, resample, treatment, splice, waveform, QC, silence, or FFmpeg behavior.

Post-read action: record specific FS04 alignment work here before changing DSP contracts, FFmpeg wrappers, audio ranges, or buffer mutation rules.

## Built-In .NET Guard Inventory

AMS Core targets `net10.0`. Before writing a throwing contract, invariant, disposal, or cancellation check, check this inventory first. These are the public .NET `ThrowIf*` methods found in the .NET 10 reference surface.

Do not add custom `Guard`, `*Guard`, or `ThrowIf` helper classes/functions for ordinary constructor or method contracts or state invariants. The invariant should be visible where it is enforced. Use the built-in guard that directly matches the invariant. If no built-in guard matches, write an explicit local `if` and throw the standard exception at the boundary that owns the contract. Use validators, parsers, or result shapes for untrusted input and expected domain rejection; do not turn normal input errors into guard exceptions.

### Guards Versus Validators

Guards are for programmer errors, trusted-state corruption, impossible object states, and lifecycle misuse. Validators are for user choices, host configuration, CLI arguments, Workstation selections, external payloads, and contextual business policy that can be rejected during normal operation. A guard may throw; a validator should usually return a reportable result, issue list, or typed rejection.

In the current AMS app, the user-selected workspace path is an input boundary. Once a workspace has been accepted, chapter-open requests built from discovered workspace state are trusted runtime requests. Missing optional artifacts are ordinary absence, not request failure.

### Argument Guards

Use these for caller contract violations on method and constructor arguments. The signatures below include the `paramName` parameter so the overload is explicit. In normal AMS code, omit `paramName` and let the .NET `CallerArgumentExpression` feature capture the argument name. Pass `paramName` only when validating a transformed/local value but reporting the original public parameter.

| Invariant | Built-in guard |
|---|---|
| Reference argument must not be null | `ArgumentNullException.ThrowIfNull(argument, paramName = null)` |
| Pointer argument must not be null | `ArgumentNullException.ThrowIfNull(argument, paramName = null)` pointer overload |
| String argument must not be null or empty | `ArgumentException.ThrowIfNullOrEmpty(argument, paramName = null)` |
| String argument must not be null, empty, or whitespace | `ArgumentException.ThrowIfNullOrWhiteSpace(argument, paramName = null)` |
| Comparable argument must not equal a value | `ArgumentOutOfRangeException.ThrowIfEqual(value, other, paramName = null)` |
| Comparable argument must equal a value | `ArgumentOutOfRangeException.ThrowIfNotEqual(value, other, paramName = null)` |
| Comparable argument must be less than or equal to a maximum | `ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other, paramName = null)` |
| Comparable argument must be less than a maximum | `ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, other, paramName = null)` |
| Comparable argument must be greater than or equal to a minimum | `ArgumentOutOfRangeException.ThrowIfLessThan(value, other, paramName = null)` |
| Comparable argument must be greater than a minimum | `ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, other, paramName = null)` |
| Numeric argument must be non-negative | `ArgumentOutOfRangeException.ThrowIfNegative(value, paramName = null)` |
| Numeric argument must be positive | `ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, paramName = null)` |
| Numeric argument must be non-zero | `ArgumentOutOfRangeException.ThrowIfZero(value, paramName = null)` |

### State And Lifecycle Guards

Use these when the object or operation state owns the failure, not a caller argument range.

| Invariant | Built-in guard |
|---|---|
| Instance must not be disposed | `ObjectDisposedException.ThrowIf(condition, instance)` |
| Type-owned resource must not be disposed | `ObjectDisposedException.ThrowIf(condition, type)` |
| Operation must stop when cancellation is requested | `cancellationToken.ThrowIfCancellationRequested()` |

### Specialized BCL Guards

These are built into specific .NET APIs. Use them only when working directly with those API types.

| API invariant | Built-in guard |
|---|---|
| ASN.1 reader must have consumed all remaining data | `AsnReader.ThrowIfNotEmpty()` |
| Server-sent-event parser must be enumerated only once | `SseParser<T>.ThrowIfNotFirstEnumeration()` |

### When No Built-In Guard Exists

Some AMS invariants are real but have no matching .NET `ThrowIf*` helper. Keep those checks inline and explicit:

- invalid file names or path separators;
- non-finite numeric values such as `NaN` or infinity;
- cross-field rules such as `end > start`;
- collection-specific rules beyond null, empty, and count checks;
- domain membership rules such as known artifact kind, known module id, or valid chapter mapping.

Do not hide those checks behind a custom guard abstraction. The developer reading the function should see every invariant the function owns.

## Scope

FS04 owns lower-level audio processing and FFmpeg interop.

Runtime buffer identity and artifact lifecycle belong to FS01. Concrete audio operations belong here.

## Current Concepts

- Audio buffers are shared artifact contracts, but concrete DSP operations live in this slice.
- Audio processors provide decode, encode, trim, analysis, loudness, activity tracking, and silence detection.
- FFmpeg wrapper classes isolate unsafe FFmpeg.AutoGen primitives and filter graphs.
- Treatment, splice, QC, silence chunking, and boundary selection prepare audio for workstation and pipeline use.

## Specific Changes Needed

- Keep decode, trim, resample, encode, treatment, splice, waveform, QC, and FFmpeg policy in FS04-owned operations.
- Define a clear audio range value if repeated operations need one; it must reject non-finite values and `end <= start`.
- Keep buffer mutation bounded behind narrow methods instead of pushing allocation-heavy immutable transitions into hot DSP paths.
- Decide whether the empty FFmpeg resampler placeholder is dead code or near-term planned work.
- Audit duplicate downmix, silence, and timeline projection rules across ASR, treatment, and prosody.
- Replace FFmpeg trim calls with zero-copy `AudioBuffer` slices when the operation only needs a range from an already-loaded buffer.
- Avoid duplicate audio materialization: prefer one decode per analysis workflow, one encode per durable artifact, and direct file streaming when an existing compatible WAV artifact already exists.
- Keep browser streaming paths explicit: stream existing artifact files from resolved audio descriptors; encode decoded buffers only when the response is computed from samples.

## Decisions

### 2026-05-16 - Decode And Resample Policy Is FS04-Owned

Decode, trim, resample, treatment, splice, waveform, QC, and FFmpeg policy should not live on FS01 runtime descriptors.

FS01 may identify that a chapter has raw, treated, corrected, or filtered audio artifacts. FS04 owns how those artifacts are decoded, sliced, resampled, encoded, or transformed for a specific operation.

### 2026-05-16 - Audio Slice Ranges Are Operation-Owned

Workstation currently slices audio through explicit operation-local values:

- region playback builds decode options from route/query start and duration;
- waveform thumbnails decode an optional request range;
- pickup source cache slices a decoded source buffer by explicit start/end;
- preview and audition flows trim loaded buffers from computed ranges;
- export and undo flows trim from operation arguments.

That supports keeping `AudioBufferDescriptor.Start` and `AudioBufferDescriptor.Duration` out of FS01. If a reusable audio range value is introduced, it should prove finite values and `end > start` in the operation that owns the range.

### 2026-05-17 - Prefer Slices And Single Materialization For Audio Ranges

When an operation already has an `AudioBuffer` and only needs a contiguous time range, it should use a backing-store slice instead of routing through FFmpeg `atrim`. FFmpeg trim remains appropriate when the operation is actually decoding from disk with a start/duration, applying filters, changing timing, resampling, or crossing an external process boundary.

Audio operations should also avoid redundant decode/encode passes. A workflow should decode once and pass the decoded buffer through analysis steps when possible. If a workflow has already created a compatible WAV artifact, downstream consumers should reuse that artifact instead of encoding the same samples again. If an endpoint is serving a full existing WAV artifact without transformation, prefer file streaming over re-encoding a cached buffer.

Streaming policy should preserve the distinction between file artifacts and decoded buffers. A full existing WAV/MP3/FLAC/M4A artifact should stream from the resolved audio descriptor path without forcing lazy buffer load. A decoded `AudioBuffer` should be encoded to a browser-readable stream only when the response is computed from samples, such as a slice, preview buffer, region fallback, waveform-adjacent operation, or transformed audio.

Do not rediscover audio file paths in streaming callsites from chapter names, stems, or working-directory guesses. Use the runtime audio context descriptor path once FS01 has resolved the artifact. Ad hoc file resolution belongs at the artifact discovery boundary, not in HTTP streaming helpers.

The current audit found likely slice replacements in:

- audio splice before/after extraction for replace, delete, and insert;
- treatment decorator, title, and content extraction;
- workstation polish audition and context preview clips;
- undo backup segment export;
- polish verification segment extraction before ASR prep;
- splice-boundary search-window extraction;
- MFA corpus chunk construction when pre-sliced chunk audio is unavailable.

The current audit found likely decode/encode materialization reductions in:

- chunked ASR, where chunk WAV artifacts are emitted for MFA reuse; keep Whisper.NET transcription on in-memory slices when the buffer is already materialized, and use prepared WAV files for WhisperX/no-buffer cases;
- benchmark metrics, where raw/treated audio is decoded for integrity and loudness, then QC decodes the same files again;
- pickup ASR/MFA, where planned chunks are transcribed from buffer slices and later encoded again for MFA corpus input;
- full chapter playback endpoints, where an existing audio artifact should be streamed from the runtime descriptor path when no range or transformation is requested;
- CRX export, where a sliced segment can be encoded directly to the destination file instead of first materializing a `MemoryStream`;
- MFA workflow corpus setup, where full-audio decode should be lazy and avoided when reusable chunk-audio artifacts satisfy the corpus request.

Current near-term backlog from the CLI/pipeline audit:

- make chunked MFA corpus construction lazy-load chapter audio only when reusable ASR chunk WAV artifacts are unavailable;
- keep Whisper.NET chunk ASR on in-memory slices when the buffer is already materialized; use file-backed prepared WAV transcription only when it avoids materialization or is required by WhisperX;
- later, collapse benchmark/QC/loudness file analysis so raw and treated files are not decoded multiple times for the same metrics pass.

We are going to try to address the FS04-owned optimizations during this pass. MFA-specific leftovers that require a broader forced-alignment redesign should remain visible here and be completed during FS06.

## Code Sketches

No code sketches recorded yet.

## Open Audit Questions

- Should audio ranges be represented as seconds, samples, or separate values for each operation?
- Where should decode options validate finite non-negative start/duration?
- Is the empty FFmpeg resampler placeholder dead code or a near-term planned implementation?
- Which downmix, silence, and timeline projection rules are duplicated across ASR, treatment, and prosody?
- Which chunk audio artifacts are durable requirements, and which exist only because an ASR or MFA boundary currently requires WAV materialization?

## Cross-Slice Boundaries

- FS01 owns runtime artifact identity and lazy load/cache lifecycle.
- FS03 owns ASR engine/model selection and recognition flow.
- FS06 owns the forced-alignment workflow shape, corpus reuse policy, and TextGrid aggregation. FS04 owns the low-level decode, encode, slice, and FFmpeg operations used by that workflow.
- FS10 owns pause dynamics semantics; FS04 owns low-level audio operations used by pause workflows.
