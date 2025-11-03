# AMS Project Memory

## Project Overview
Audio Management System (AMS) - CLI and core library for audio processing, ASR, forced alignment, and audiobook mastering.

## Current Pipeline (MFA-based)

The pipeline uses **Nemo ASR** for initial speech recognition and **Montreal Forced Aligner (MFA)** for precise word/phone-level timing refinement.

### Pipeline Stages

1. **ASR Stage** (`host/Ams.Cli/Commands/AsrCommand.cs`)
   - Uses Nemo ASR service to generate initial word-level timings
   - Outputs: `{chapter}.asr.json` with token timings

2. **Alignment Stage** (`host/Ams.Cli/Commands/AlignCommand.cs`)
   - Anchor selection: identifies reliable sync points between book text and ASR output
   - Transcript indexing: matches book text to ASR tokens
   - Hydration: creates fully timed transcript from book source
   - Outputs: `{chapter}.align.anchors.json`, `{chapter}.align.tx.json`, `{chapter}.align.hydrate.json`

3. **MFA Forced Alignment** (`host/Ams.Cli/Services/MfaWorkflow.cs`)
   - Validates corpus and identifies out-of-vocabulary (OOV) words
   - Generates pronunciations for OOV words using G2P (grapheme-to-phoneme)
   - Creates custom dictionary with OOV pronunciations
   - Runs forced alignment to produce TextGrid with precise word/phone boundaries
   - Outputs: `{chapter}.TextGrid`, alignment analysis, and supporting artifacts

4. **Timing Merge** (`host/Ams.Core/Alignment/Mfa/MfaTimingMerger.cs`)
   - Merges MFA TextGrid timings into hydrate and tx JSON files
   - Replaces initial ASR timings with more accurate forced-alignment timings

5. **Roomtone Rendering** (`host/Ams.Core/Pipeline/RoomToneInsertionStage.cs`)
   - Uses TextGrid gap hints for improved silence detection
   - Fills gaps between sentences with generated room tone
   - Configurable gap detection parameters (thresholds, step, backoff)
   - Outputs: `{chapter}.treated.wav`

### Key Models & Services

- **Nemo ASR**: Default service at `http://localhost:8765`
- **MFA Models**:
  - Dictionary: `english_us_arpa`
  - Acoustic: `english_us_arpa`
  - G2P: `english_us_arpa`

### Usage (PowerShell)

```powershell
# Run full pipeline (ASR → Alignment → MFA → Roomtone)
dotnet .\host\Ams.Cli\bin\Debug\net9.0\Ams.Cli.dll pipeline run `
  --book "C:\Books\MyBook.md" `
  --audio "C:\Audio\Chapter01.wav" `
  --work-dir "C:\Work" `
  --chapter "Chapter 01"

# Individual stages
# 1. ASR only
dotnet .\host\Ams.Cli\bin\Debug\net9.0\Ams.Cli.dll asr run `
  --audio "C:\Audio\Chapter01.wav" `
  --output "C:\Work\Chapter01.asr.json"

# 2. Alignment (requires book index and ASR output)
dotnet .\host\Ams.Cli\bin\Debug\net9.0\Ams.Cli.dll align hydrate `
  --book-index "C:\Work\book-index.json" `
  --asr "C:\Work\Chapter01.asr.json" `
  --transcript "C:\Work\Chapter01.align.tx.json" `
  --output "C:\Work\Chapter01.align.hydrate.json"

# 3. Roomtone rendering with custom gap parameters
dotnet .\host\Ams.Cli\bin\Debug\net9.0\Ams.Cli.dll audio render `
  --tx "C:\Work\Chapter01.align.tx.json" `
  --output "C:\Work\Chapter01.treated.wav" `
  --gap-left-threshold-db -30 `
  --gap-right-threshold-db -30 `
  --gap-step-ms 5 `
  --gap-backoff-ms 5
```

### Notes
- MFA runs in a persistent PowerShell session with conda environment activated (via `MfaProcessSupervisor`)
- TextGrid files provide phone-level timing precision for improved gap detection
- Gap calibration steps inward until RMS drops below silence threshold
- All gaps are clamped to neighboring sentence timings to prevent overlap with speech
