# AMS Project Memory

## Project Overview
Audio Management System (AMS) - CLI and core library for audio processing, ASR refinement, and transcript alignment.


## Sentence-Only Refinement (Current)

We removed token-level timing refinement entirely. Refinement clamps only SENTENCE boundaries:

- Starts: Aeneas per-sentence alignment (one text line per sentence from TX ScriptRanges).
- Ends: FFmpeg `silencedetect` using a user-provided noise floor; choose nearest silence end after the sentence and before the next start.

### Components
- SentenceRefinementService (`host/Ams.Core/SentenceRefinementService.cs`) → emits `[ { start, end, startWordIdx, endWordIdx } ]`.
- RefineSentencesCommand (`host/Ams.Cli/Commands/RefineSentencesCommand.cs`) → CLI wrapper.
- Roomtone renderer accepts `--sentences` JSON to enforce these timings when filling gaps.

### Usage (PowerShell)
```powershell
# Refine sentence boundaries
dotnet .\host\Ams.Cli\bin\Debug\net9.0\Ams.Cli.dll refine-sentences `
  -t "C:\transcriptions\Book\Chapter 14.tx.json" `
  -j "C:\transcriptions\Book\Chapter 14.asr.json" `
  -a "C:\audio\Chapter 14.wav" `
  -o "C:\transcriptions\Book\Chapter 14.sentences.refined.json" `
  --language eng `
  --silence-threshold-db -38 `
  --silence-min-dur 0.12

# Render roomtone using refined sentence times
dotnet .\scripts\RoomtoneCli\bin\Release\net9.0\RoomtoneCli.dll `
  "C:\transcriptions\Book\Chapter 14.tx.json" `
  "C:\transcriptions\Book\Chapter 14.roomtone.wav" `
  --sentences "C:\transcriptions\Book\Chapter 14.sentences.refined.json" `
  --sr 44100 --fade 5 --tone -60
```

Notes
- Silence detection is assumed; pass your noise floor with `--silence-threshold-db` (default -30 dBFS). No token timing changes.

## Agent Configuration

**Default Agent Plan**: `.agents/plan.json` (pinned configuration)

This file contains the canonical agent orchestration configuration following the Handoff Contract schema defined in `agents.md`. The plan includes:
- MCP execution mode with proper tool whitelisting
- Sequential thinking with GPT-5-Codex implementer  
- Sandbox and approval policies
- Acceptance test definitions

See `agents.md` for the complete schema specification and usage guidelines.
