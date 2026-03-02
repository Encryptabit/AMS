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

`Ams.Core` now runs this precheck before `Build`/`Publish` and warns when FFmpeg is missing.
- Disable precheck: `/p:AmsFfmpegPrecheck=false`
- Make missing FFmpeg fail the build: `/p:AmsFfmpegPrecheckFailBuild=true`
