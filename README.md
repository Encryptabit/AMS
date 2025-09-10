# AMS
AudioBook Mastering Suite using Zig + AvaloniaUI

Quick start (pipeline)
- Build book index (required):
  dotnet run --project host/Ams.Cli -- build-index --book <book.docx|.txt|.md|.rtf> --out <input>.ams/book.index.json
- Run stages (use --from/--to as needed):
  dotnet run --project host/Ams.Cli -- asr run --book <book.docx|.txt|.md|.rtf> --in <audio.wav> --work <input>.ams

Key verbs
- asr detect-silence, asr plan-windows, asr run, asr refine, asr collate, validate-manifest
- Book tools: build-index, book verify
