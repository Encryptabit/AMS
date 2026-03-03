# AMS
AudioBook Mastering Suite using Zig + AvaloniaUI

## FFmpeg Setup
AMS expects FFmpeg shared libraries under:
- `host/ExtTools/ffmpeg/bin`
- `host/Ams.Core/ExtTools/ffmpeg/bin`

Install/update pinned FFmpeg binaries with one cross-platform command:

```bash
python3 scripts/setup_ffmpeg.py
```

On Windows (if `python3` is not on PATH):

```powershell
py .\scripts\setup_ffmpeg.py
```

Preview actions without changing files:

```bash
python3 scripts/setup_ffmpeg.py --dry-run
```

Validate existing local install only (no download):

```bash
python3 scripts/setup_ffmpeg.py --check-only
```

`Ams.Core` now runs this precheck before `Build`/`Publish`.
- If FFmpeg is missing, it auto-installs by default (`AmsFfmpegAutoInstall=true`).
- Disable precheck: `/p:AmsFfmpegPrecheck=false`
- Disable auto-install fallback: `/p:AmsFfmpegAutoInstall=false`
- Make precheck/install failures fail the build: `/p:AmsFfmpegPrecheckFailBuild=true`

## CRX Page Autofill
Populate missing CRX page numbers from a print PDF using chapter-constrained fuzzy matching:

```bash
uv run --with openpyxl --with pypdf --with rapidfuzz \
  python scripts/fill_crx_pdf_pages.py \
    --xlsx "E:/Audiobooks/Raws/My Book/CRX/My Book_CRX.xlsx" \
    --pdf "E:/Audiobooks/Raws/My Book/My-Print.pdf" \
    --book-index "E:/Audiobooks/Raws/My Book/book-index.json" \
    --dry-run
```

Write updates (creates a timestamped backup by default):

```bash
uv run --with openpyxl --with pypdf --with rapidfuzz \
  python scripts/fill_crx_pdf_pages.py \
    --xlsx "E:/Audiobooks/Raws/My Book/CRX/My Book_CRX.xlsx" \
    --pdf "E:/Audiobooks/Raws/My Book/My-Print.pdf" \
    --book-index "E:/Audiobooks/Raws/My Book/book-index.json"
```
