# FS04: Audio DSP, QC, And FFmpeg Integration

Last updated: 2026-05-16

Reader: an engineer changing audio decode, encode, trim, resample, treatment, splice, waveform, QC, silence, or FFmpeg behavior.

Post-read action: record specific FS04 alignment work here before changing DSP contracts, FFmpeg wrappers, audio ranges, or buffer mutation rules.

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

## Code Sketches

No code sketches recorded yet.

## Open Audit Questions

- Should audio ranges be represented as seconds, samples, or separate values for each operation?
- Where should decode options validate finite non-negative start/duration?
- Is the empty FFmpeg resampler placeholder dead code or a near-term planned implementation?
- Which downmix, silence, and timeline projection rules are duplicated across ASR, treatment, and prosody?

## Cross-Slice Boundaries

- FS01 owns runtime artifact identity and lazy load/cache lifecycle.
- FS03 owns ASR engine/model selection and recognition flow.
- FS10 owns pause dynamics semantics; FS04 owns low-level audio operations used by pause workflows.
