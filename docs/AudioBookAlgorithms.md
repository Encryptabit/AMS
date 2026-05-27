# AudioBookAlgorithms

Reader: AMS engineers designing or revisiting algorithms that are specific to
spoken audiobook performance.

Post-read action: add, compare, or refine audiobook-specific algorithm ideas
without rediscovering prior intent from code or chat history.

This document is a living notebook. It records algorithm intent, current
implementation status, expected inputs and outputs, and unresolved design
questions. It is not a command reference.

## Entry Template

Use this shape for new ideas:

- Name:
- Status: Idea, prototype, implemented, retired, or superseded.
- Performance problem:
- Listener-facing goal:
- Inputs:
- Output artifact or behavior:
- Algorithm sketch:
- Tuning controls:
- Failure modes:
- Open questions:

## Prosody Compression

Status: implemented as pause dynamics.

Prosody Compression is a timing algorithm, not an amplitude compressor. Its
current implementation changes pause durations in the aligned transcript
timeline so a spoken performance keeps a consistent audiobook pace without
flattening every intentional dramatic pause.

### Performance Problem

Narrators often produce pauses that are locally expressive but globally uneven:
some sentence gaps run long, some paragraph gaps are too short, and some
intra-sentence silences create drag. A simple fixed-duration pause replacement
would sound mechanical. The better behavior is to compress pause durations
toward class-specific windows while preserving meaningful outliers.

### Listener-Facing Goal

The listener should hear cleaner pacing, not obvious editing. Sentence, comma,
paragraph, chapter, and tail pauses should each feel internally consistent while
still allowing the narrator's performance to breathe.

### Inputs

- Aligned sentence timing from the transcript.
- Book structure from the indexed script, especially paragraph and heading
  boundaries.
- Optional hydrated sentence text for punctuation-aware intra-sentence gaps.
- Optional TextGrid silence intervals for detected intra-sentence pauses.
- A pause policy with class windows and compression controls.

### Current Algorithm

The current system builds a pause analysis from the chapter timeline, then
plans and applies pause adjustments.

1. Detect inter-sentence spans from the end of one sentence to the start of the
   next sentence.
2. Classify those spans as sentence or paragraph pauses based on book structure.
3. Detect intra-sentence spans from silence intervals, with comma punctuation
   used as a placement hint when requested.
4. Build per-class compression profiles from observed pause durations.
5. Preserve the configured top-duration quantile so the longest expressive
   pauses are not automatically shortened.
6. Compress each remaining duration toward the configured class window with a
   soft-knee profile.
7. Project the adjusted durations back onto the sentence timeline.

Inter-sentence adjustments shift the right sentence and every following
sentence. Intra-sentence adjustments update the owning sentence duration,
record the adjusted internal gap, and shift following sentences by the same
delta.

### Default House Policy

| Pause class | Target |
| --- | --- |
| Comma | 0.20s to 0.50s |
| Sentence | 0.60s to 1.00s |
| Paragraph | 1.10s to 1.40s |
| Chapter head | 0.75s fixed |
| Post-chapter read | 1.50s fixed |
| Tail | 3.00s fixed |

Default compression controls:

- Knee width: 0.08s.
- Ratio inside the target window: 1.25.
- Ratio outside the target window: 3.0.
- Preserve top quantile: 0.95.

### Current Safeguards

- Spans crossing chapter headings are skipped by the planner.
- The execution path filters ordinary adjustments touching paragraph zero.
- Intra-sentence shrink is capped so comma-level edits cannot remove too much
  speech-adjacent timing in one pass.
- Intra-sentence detection adds small edge guards around detected silence spans.
- The transform model contains breath-cut support, but the current planner does
  not emit breath cuts.

### Existing Surfaces

- Pause policy files can be scaffolded and loaded at chapter or book scope.
- Interactive timing validation runs pause dynamics analysis by default.
- Pipeline statistics can report prosody pause stats.
- Pause-adjusted audio is represented by the adjusted audio tier for staging and
  QC workflows.

### Open Questions

- Should the term Prosody Compression remain the umbrella name, with pause
  dynamics as the first implementation, or should the product language use
  Pause Dynamics everywhere?
- Should breath cuts become part of this algorithm or remain a separate
  audiobook algorithm?
- Should perceptual masking influence which micro-pauses or breath artifacts
  are safe to alter?
- Should paragraph and chapter pacing use book-level distributions instead of
  only chapter-local pause statistics?
- Should the adjusted tier eventually represent a stack of timing algorithms,
  or only this pause-compression pass?

## Segment-Relative Dynamic Normalization

Status: idea.

Segment-Relative Dynamic Normalization is an audiobook-specific loudness
algorithm. It should make a performance easier to listen to without erasing the
narrator's intentional relative dynamics between scene, sentence, and character
segments.

### Performance Problem

Audiobook narration can have short passages that are much louder than the rest
of a chapter, while the chapter's integrated loudness remains too low. A
chapter-level normalizer can hit delivery targets, but it may lift everything
equally or clamp the loudest passage so hard that the performance feels flat.
A frame-level dynamic normalizer can solve the numbers while making scene-level
dynamics pump or drift.

The better behavior is to normalize against performance segments. A whispered
line can remain softer than a shouted line, but neither should make the rest of
the chapter miss the target.

### Listener-Facing Goal

The listener should not need to adjust volume within a chapter. At the same
time, the narrator's intentional loud, quiet, close, distant, or character
specific delivery should remain recognizable.

### Inputs

- The source or filtered audio buffer.
- A sentence or phrase timeline from alignment.
- Optional scene, paragraph, chapter, or speaker/character boundaries.
- Loudness and peak measurements per segment and per chapter.
- Delivery targets such as integrated loudness, true peak, and minimum/maximum
  loudness range.

### Algorithm Sketch

1. Partition the chapter into performance segments. The first practical version
   can use sentence and paragraph boundaries; later versions can use dialogue,
   speaker, or scene structure.
2. Measure loudness, RMS, true peak, and crest factor per segment.
3. Estimate a chapter target gain that gets the whole artifact near delivery
   spec.
4. Estimate local correction only for outlier segments that would dominate or
   disappear after the chapter gain.
5. Smooth gain changes across segment boundaries so the correction is not heard
   as pumping.
6. Preserve relative ordering: if segment A was intentionally quieter than
   segment B, normalization should avoid crossing those levels unless the
   difference is outside configured bounds.
7. Apply a final true-peak limiter only as a safety ceiling, not as the primary
   normalizer.

### Tuning Controls

- Segment source: sentence, paragraph, scene, speaker, or hybrid.
- Target integrated loudness.
- True-peak ceiling.
- Maximum local gain boost and attenuation.
- Maximum allowed compression of inter-segment loudness contrast.
- Smoothing window across segment boundaries.
- Minimum segment duration before independent correction is allowed.

### Failure Modes

- Treating every sentence as independent can create audible gain stepping.
- Over-preserving relative dynamics can leave the chapter outside delivery
  loudness targets.
- Over-normalizing can erase narrator intent, especially whispers, shouts,
  asides, and character voices.
- Peak-heavy passages can make the target loudness impossible without additional
  dynamic range control.

### Open Questions

- Should the first implementation use sentence boundaries, paragraph
  boundaries, or pause-class spans from Prosody Compression?
- Should the algorithm emit a gain envelope artifact so Workstation can show
  what it changed?
- Should the target be chapter-local, book-local, or delivery-preset based?
- Should the algorithm run before or after defect cleaning?
- How should it report impossible targets where true peak and integrated
  loudness cannot both be satisfied without stronger compression?

## Perceptual Masking Defect Cleaning

Status: idea.

Perceptual Masking Defect Cleaning is an audiobook cleaning strategy for clicks,
plosives, mouth noise, page noise, and other short defects. It should use voice
activity and masking context to decide where cleaning is worth doing, not only
whether a waveform feature looks suspicious.

### Performance Problem

The same defect can be obvious in roomtone and irrelevant under active speech.
A global declicker or deplosive pass can waste effort on masked artifacts while
risking damage to consonants, breaths, and performance texture. Spoken-word
audio needs more context than a raw transient detector can provide.

### Listener-Facing Goal

Cleaning should target defects the listener is likely to notice. Quiet spaces,
pause tails, breaths between phrases, and exposed roomtone should receive
stricter cleanup. Active speech should be protected unless a defect is strong
enough to rise above the speech mask.

### Inputs

- The audio buffer to clean.
- A voice activity map, ideally at frame or word resolution.
- Sentence, word, pause, and roomtone spans from alignment and silence analysis.
- Defect candidates from detectors such as declick, declip, deplosive,
  mouth-noise, or broadband transient detection.
- Optional spectral features: frequency band energy, flatness, zero crossing,
  crest factor, and local noise floor.

### Algorithm Sketch

1. Build a voice activity and pause map for the chapter.
2. Detect candidate defects with specialized detectors.
3. Score each candidate by context:
   - highest priority in roomtone, inter-sentence gaps, and pause tails;
   - medium priority near breaths or low-energy speech;
   - lower priority under active voiced speech where masking is strong.
4. Estimate audibility from defect level relative to local speech energy, local
   noise floor, and frequency-band masking.
5. Choose repair strength from the audibility score. Exposed defects can use
   stronger interpolation or attenuation; masked defects can be skipped or
   repaired conservatively.
6. Protect speech-like transients by requiring stronger evidence inside voiced
   regions, especially around plosives, fricatives, and consonant attacks.
7. Emit a cleaning decision artifact so each repair can be audited.

### Tuning Controls

- Voice activity threshold.
- Pause and roomtone strictness.
- Minimum defect-to-mask ratio for active speech.
- Repair aggressiveness per region type.
- Speech transient protection strength.
- Defect class priorities.
- Maximum repair density per second to prevent over-processing.

### Failure Modes

- Weak voice activity detection can misclassify breathy narration as roomtone.
- Over-aggressive cleanup in silence can remove intentional breath or room
  character.
- Under-aggressive cleanup during speech can leave obvious mouth clicks in quiet
  close-mic performances.
- Detector-specific repairs can conflict if multiple cleaners target the same
  sample range.

### Open Questions

- Should voice activity come from a dedicated VAD, alignment confidence, RMS
  gates, or a hybrid?
- Should repairs be grouped by pause span so Workstation can preview before and
  after each exposed defect cluster?
- Should the cleaning decision artifact store skipped candidates as well as
  repaired candidates?
- Should perceptual masking scores influence automatic QC flags?
- Should defect cleaning run before loudness normalization, after it, or in two
  passes with different thresholds?
