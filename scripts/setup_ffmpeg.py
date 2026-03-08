#!/usr/bin/env python3
"""
Install FFmpeg shared binaries for AMS on Windows and Linux.

Why this script exists:
- Keep FFmpeg binaries out of git.
- Provide one cross-platform setup command for local clones.
"""

from __future__ import annotations

import argparse
import os
import shutil
import sys
import tarfile
import tempfile
import urllib.request
import zipfile
from pathlib import Path

PLATFORM_ARTIFACTS: dict[str, dict[str, str]] = {
    "windows": {
        "archive_name": "ffmpeg-n8.0-latest-win64-gpl-shared-8.0.zip",
        "archive_url": (
            "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/"
            "ffmpeg-n8.0-latest-win64-gpl-shared-8.0.zip"
        ),
    },
    "linux": {
        "archive_name": "ffmpeg-n8.0-latest-linux64-gpl-shared-8.0.tar.xz",
        "archive_url": (
            "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/"
            "ffmpeg-n8.0-latest-linux64-gpl-shared-8.0.tar.xz"
        ),
    },
}

REQUIRED_ALIAS_PREFIXES = (
    "avcodec",
    "avformat",
    "avutil",
    "avfilter",
    "swresample",
    "swscale",
)


def required_prefixes_for_platform(platform_key: str) -> tuple[str, ...]:
    if platform_key == "windows":
        return ("avcodec", "avformat", "avutil", "avfilter")
    return REQUIRED_ALIAS_PREFIXES


def has_required_native_libraries(directory: Path, platform_key: str) -> bool:
    if not directory.is_dir():
        return False

    names = [p.name.lower() for p in directory.iterdir() if p.is_file()]
    required = required_prefixes_for_platform(platform_key)
    for prefix in required:
        if platform_key == "windows":
            found = any(name.startswith(prefix) and name.endswith(".dll") for name in names)
        else:
            found = any(
                name.startswith(f"{prefix}.so") or name.startswith(f"lib{prefix}.so")
                for name in names
            )
        if not found:
            return False

    return True


def find_valid_install(destinations: list[Path], platform_key: str) -> Path | None:
    for destination in destinations:
        if has_required_native_libraries(destination, platform_key):
            return destination
    return None


def download_file(url: str, destination: Path) -> None:
    destination.parent.mkdir(parents=True, exist_ok=True)
    with urllib.request.urlopen(url) as response, destination.open("wb") as out:
        shutil.copyfileobj(response, out)


def safe_extract_zip(archive: Path, destination: Path) -> None:
    with zipfile.ZipFile(archive, "r") as zf:
        for member in zf.infolist():
            resolved = (destination / member.filename).resolve()
            if not str(resolved).startswith(str(destination.resolve())):
                raise RuntimeError(f"Unsafe zip entry path: {member.filename}")
        zf.extractall(destination)


def safe_extract_tar_xz(archive: Path, destination: Path) -> None:
    with tarfile.open(archive, "r:xz") as tf:
        for member in tf.getmembers():
            resolved = (destination / member.name).resolve()
            if not str(resolved).startswith(str(destination.resolve())):
                raise RuntimeError(f"Unsafe tar entry path: {member.name}")
        tf.extractall(destination)


def detect_platform(explicit: str | None) -> str:
    if explicit:
        return explicit
    if os.name == "nt":
        return "windows"
    if sys.platform.startswith("linux"):
        return "linux"
    raise RuntimeError(
        "Unsupported platform for this script. Use --platform windows|linux."
    )


def ensure_expected_layout(extract_root: Path) -> tuple[Path, Path]:
    roots = [p for p in extract_root.iterdir() if p.is_dir()]
    if len(roots) != 1:
        raise RuntimeError(
            f"Expected one extracted root directory, found {len(roots)} under {extract_root}"
        )
    payload_root = roots[0]
    payload_bin = payload_root / "bin"
    payload_lib = payload_root / "lib"
    if not payload_bin.is_dir():
        raise RuntimeError(f"Missing bin directory in extracted payload: {payload_bin}")
    if not payload_lib.is_dir():
        raise RuntimeError(f"Missing lib directory in extracted payload: {payload_lib}")
    return payload_bin, payload_lib


def clean_directory(path: Path) -> None:
    if path.exists():
        shutil.rmtree(path)
    path.mkdir(parents=True, exist_ok=True)


def copy_files(files: list[Path], destination: Path) -> None:
    for source in files:
        if source.is_dir():
            continue
        shutil.copy2(source, destination / source.name)


def create_linux_aliases(destination: Path) -> None:
    """
    FfSession's probe checks for files that start with names like `avcodec`.
    Linux shared libs are usually named `libavcodec.so*`, so create aliases.
    """
    for prefix in REQUIRED_ALIAS_PREFIXES:
        candidates = sorted(destination.glob(f"lib{prefix}.so*"))
        if not candidates:
            continue

        preferred = None
        for candidate in candidates:
            if candidate.name == f"lib{prefix}.so":
                preferred = candidate
                break
        if preferred is None:
            preferred = candidates[0]
        assert preferred is not None

        alias = destination / f"{prefix}.so"
        if alias.exists():
            alias.unlink()

        try:
            alias.symlink_to(preferred.name)
        except OSError:
            # Fallback for filesystems without symlink support.
            shutil.copy2(preferred, alias)


def install_payload(
    platform_key: str,
    payload_bin: Path,
    payload_lib: Path,
    destinations: list[Path],
    dry_run: bool,
) -> None:
    if platform_key == "windows":
        source_files = [p for p in payload_bin.iterdir() if p.is_file()]
    else:
        source_files = [p for p in payload_lib.iterdir() if p.is_file()]
        source_files.extend([p for p in payload_bin.iterdir() if p.is_file()])

    if dry_run:
        print("Dry run: would install files:")
        for src in sorted(source_files):
            print(f"  - {src.name}")
        print("Dry run: target directories:")
        for dst in destinations:
            print(f"  - {dst}")
        return

    for destination in destinations:
        clean_directory(destination)
        copy_files(source_files, destination)
        if platform_key == "linux":
            create_linux_aliases(destination)


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Install FFmpeg shared binaries for AMS."
    )
    parser.add_argument(
        "--platform",
        choices=("windows", "linux"),
        help="Force platform artifact selection. Default: auto-detect.",
    )
    parser.add_argument(
        "--repo-root",
        default=str(Path(__file__).resolve().parents[1]),
        help="AMS repo root (defaults to script parent parent).",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Show actions without writing files.",
    )
    parser.add_argument(
        "--check-only",
        action="store_true",
        help="Validate local FFmpeg install only; do not download or write files.",
    )
    args = parser.parse_args()

    platform_key = detect_platform(args.platform)
    artifact = PLATFORM_ARTIFACTS[platform_key]
    repo_root = Path(args.repo_root).resolve()

    if not (repo_root / ".git").exists():
        raise RuntimeError(f"Not a git repo root: {repo_root}")

    destinations = [
        repo_root / "host" / "ExtTools" / "ffmpeg" / "bin",
        repo_root / "host" / "Ams.Core" / "ExtTools" / "ffmpeg" / "bin",
    ]

    print(f"Platform: {platform_key}")
    print(f"Repo root: {repo_root}")
    print(f"Archive: {artifact['archive_name']}")

    if args.check_only:
        found = find_valid_install(destinations, platform_key)
        if found is not None:
            print(f"FFmpeg binaries detected: {found}")
            return 0

        print("FFmpeg binaries are missing from expected paths:", file=sys.stderr)
        for path in destinations:
            print(f"- {path}", file=sys.stderr)
        print(
            "Run this script without --check-only to install binaries.",
            file=sys.stderr,
        )
        return 2

    with tempfile.TemporaryDirectory(prefix="ams-ffmpeg-") as tmp_str:
        tmp = Path(tmp_str)
        archive_name = artifact["archive_name"]
        archive_path = tmp / archive_name
        extract_root = tmp / "extract"
        extract_root.mkdir(parents=True, exist_ok=True)

        print("Downloading archive...")
        download_file(artifact["archive_url"], archive_path)

        print("Extracting archive...")
        if archive_name.endswith(".zip"):
            safe_extract_zip(archive_path, extract_root)
        else:
            safe_extract_tar_xz(archive_path, extract_root)

        payload_bin, payload_lib = ensure_expected_layout(extract_root)
        install_payload(
            platform_key=platform_key,
            payload_bin=payload_bin,
            payload_lib=payload_lib,
            destinations=destinations,
            dry_run=args.dry_run,
        )

    if args.dry_run:
        print("Dry run complete.")
    else:
        print("FFmpeg install complete.")
        for path in destinations:
            print(f"- {path}")

    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:
        print(f"Error: {exc}", file=sys.stderr)
        raise SystemExit(1)
