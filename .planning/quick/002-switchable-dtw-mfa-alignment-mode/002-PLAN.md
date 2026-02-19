---
phase: quick
plan: 002
type: plan
---

<objective>
Add a selectable alignment strategy so AMS can run either chunked DTW-only timing or MFA timing per workflow/context.

Purpose: Let production throughput use DTW when acceptable, while preserving MFA for editing-grade alignment.
Output: One switchable alignment mode path with stable defaults and clear fallbacks.
</objective>

<context>
Current findings:
- Unchunked DTW in Whisper.NET 1.9 can truncate long files.
- Chunked DTW (e.g., 30s windows with overlap) restores full coverage and usable segment timing.
- MFA still provides stronger alignment quality for detailed editing.

Desired operator behavior:
- Fast mode: DTW-first for chapter throughput.
- Edit mode: MFA-first when precision matters.
</context>

<tasks>
<task type="auto">
  <name>Task 1: Introduce alignment strategy config and CLI surface</name>
  <files>host/Ams.Cli/Commands/PipelineCommand.cs, host/Ams.Cli/Commands/AsrCommand.cs, host/Ams.Core/Services/PipelineService.cs</files>
  <action>Add explicit alignment strategy options (for example: `mfa`, `dtw`, `hybrid`) and plumb the selected mode through pipeline options.</action>
  <verify>Run CLI help and a dry run parse to confirm accepted values and defaults are visible and valid.</verify>
  <done>Alignment mode can be selected per run without code edits.</done>
</task>

<task type="auto">
  <name>Task 2: Implement chunked DTW alignment path as first-class pipeline mode</name>
  <files>host/Ams.Core/Processors/AsrProcessor.cs, host/Ams.Core/Application/Commands/GenerateTranscriptCommand.cs, host/Ams.Core/Application/Commands/MergeTimingsCommand.cs</files>
  <action>Add chunked DTW timing generation and ensure downstream timing merge can consume DTW artifacts when MFA is skipped.</action>
  <verify>Process a known chapter with DTW mode and confirm full-duration timing artifacts (no truncation) and successful merge.</verify>
  <done>DTW mode completes end-to-end with deterministic output artifacts.</done>
</task>

<task type="auto">
  <name>Task 3: Add safe defaults + fallback policy for production use</name>
  <files>host/Ams.Core/Processors/AsrProcessor.cs, host/Ams.Cli/Commands/PipelineCommand.cs, docs/* (as needed)</files>
  <action>Set practical defaults (editing prefers MFA, throughput prefers DTW if selected) and add fallback behavior when DTW coverage is suspicious (rerun non-DTW or route to MFA).</action>
  <verify>Run sample chapters in each mode and confirm fallback triggers only when needed; document mode guidance.</verify>
  <done>Operators can reliably choose speed vs precision with predictable behavior.</done>
</task>
</tasks>

<verification>
- CLI mode selection is explicit and validated.
- DTW chunked mode achieves full chapter coverage on long audio.
- MFA mode remains available and unchanged for edit workflows.
</verification>

<success_criteria>
- [ ] Alignment mode switch exists and is usable from CLI/pipeline.
- [ ] DTW mode no longer truncates long chapters in supported configs.
- [ ] MFA remains selectable for high-precision editing.
- [ ] Mode guidance and fallback behavior are documented.
</success_criteria>

<output>
When executed, create:
- `.planning/quick/002-switchable-dtw-mfa-alignment-mode/002-SUMMARY.md`

Include:
- selected mode semantics and defaults
- changed files and artifact formats
- validation results on at least one long chapter
- residual limitations and follow-up items
</output>
