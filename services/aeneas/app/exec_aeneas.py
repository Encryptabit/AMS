"""
Wrapper around aeneas.tools.execute_task for forced alignment.
"""
import os
import sys
import tempfile
import subprocess
import json
import hashlib
from pathlib import Path
from typing import List, Dict, Any, Optional
import logging

logger = logging.getLogger(__name__)


def normalize_path(path: str) -> str:
    """
    Normalize Windows and WSL paths for internal use.
    Accept both C:\... and /mnt/c/... paths, normalize to appropriate format.
    """
    if not path:
        return path
    
    # Convert backslashes to forward slashes for consistency
    normalized = path.replace('\\', '/')
    
    # If it's a Windows path (C:/...), convert to WSL format when running in WSL
    if len(normalized) >= 3 and normalized[1:3] == ':/':
        # This is a Windows path like C:/...
        if os.name != 'nt':  # Running in WSL/Linux
            drive_letter = normalized[0].lower()
            rest_of_path = normalized[3:]
            normalized = f"/mnt/{drive_letter}/{rest_of_path}"
    
    return normalized


def get_python_version() -> str:
    """Get Python version string."""
    return f"{sys.version_info.major}.{sys.version_info.minor}.{sys.version_info.micro}"


def get_aeneas_version() -> str:
    """Get Aeneas version if available."""
    try:
        import aeneas
        return getattr(aeneas, '__version__', 'unknown')
    except ImportError:
        return "not-installed"


def compute_text_digest(lines: List[str]) -> str:
    """Compute a digest of the text lines for fingerprinting."""
    combined = '\n'.join(lines).strip()
    return hashlib.sha256(combined.encode('utf-8')).hexdigest()[:16]


def _build_aeneas_sensitive_config(language: str) -> str:
    """
    Sentence-level 'sensitive' preset:
    - Higher temporal resolution (smaller MFCC hop/len at L2)
    - Mask nonspeech in DTW cost matrix (prefers voiced zones)
    - Eager-ish VAD (lower threshold, tiny speech padding)
    - Stripe DTW with a bit more margin
    - Gentle boundary pull toward speech
    """
    lang = (language or "eng").strip().lower()
    parts = [
        f"task_language={lang}",
        "is_text_type=plain",
        "os_task_file_format=json",

        # DTW: allow a little more latitude without going 'exact'
        "dtw_algorithm=stripe",
        "dtw_margin_l2=45",

        # Temporal resolution at sentence level (L2)
        "mfcc_window_shift_l2=0.010",
        "mfcc_window_length_l2=0.030",

        # Prefer speech in the cost matrix
        "mfcc_mask_nonspeech_l2=True",
        "mfcc_mask_log_energy_threshold=0.50",
        "mfcc_mask_min_nonspeech_length=2",

        # VAD: catch quieter speech, avoid tiny silence islands
        "vad_log_energy_threshold=0.50",
        "vad_min_nonspeech_length=0.10",
        "vad_extend_speech_before=0.02",
        "vad_extend_speech_after=0.02",

        # Post-boundary adjustment
        "task_adjust_boundary_algorithm=beforenext",
        "task_adjust_boundary_beforenext_value=0.10",
        "task_adjust_boundary_no_zero=True",
    ]
    return "|".join(parts)


def _merge_config(base: str, overlay: Optional[str]) -> str:
    if not overlay:
        return base
    # Avoid duplicate separators or leading/trailing pipes
    base = base.strip("|")
    overlay = overlay.strip("|")
    if not base:
        return overlay
    return base + "|" + overlay

def run_aeneas_alignment(
    audio_path: str,
    text_lines: List[str],
    language: str = "eng",
    timeout_sec: int = 600,
    *,
    config: Optional[str] = None  # NEW: optional override/extra config
) -> Dict[str, Any]:
    """
    Run Aeneas forced alignment on a single audio chunk with text lines.

    Args:
        audio_path: Path to audio file (WAV format)
        text_lines: List of text lines to align
        language: ISO-639-3 (e.g., "eng", "fra")
        timeout_sec: Maximum execution time
        config: Optional aeneas config string to append/override defaults.
                If not provided, uses base defaults. If env AMS_AENEAS_PRESET=sensitive,
                applies a 'sensitive' preset automatically.

    Returns:
        Dict containing alignment fragments and metadata
    """
    if not text_lines:
        raise ValueError("text_lines cannot be empty")

    # Normalize paths for the environment we're running in
    audio_path = normalize_path(audio_path)

    if not os.path.exists(audio_path):
        raise FileNotFoundError(f"Audio file not found: {audio_path}")

    # Choose Python with aeneas available
    python_exe = os.environ.get("AENEAS_PYTHON")
    if not python_exe:
        for candidate in ["python", "python3", sys.executable]:
            try:
                result = subprocess.run(
                    [candidate, "-c", "import aeneas"],
                    capture_output=True,
                    timeout=5,
                )
                if result.returncode == 0:
                    python_exe = candidate
                    break
            except (subprocess.TimeoutExpired, FileNotFoundError):
                continue
        else:
            python_exe = sys.executable  # fallback

    logger.info(f"Running alignment: {audio_path} with {len(text_lines)} lines")
    logger.debug(f"Python executable: {python_exe}")
    logger.debug(f"Language: {language}")

    with tempfile.TemporaryDirectory() as temp_dir:
        # Write text lines to file
        text_file = os.path.join(temp_dir, "text.txt")
        with open(text_file, "w", encoding="utf-8") as f:
            for line in text_lines:
                f.write((line or "").strip() + "\n")

        # Output target
        output_file = os.path.join(temp_dir, "alignment.json")

        # --- Build config string ---
        # Base defaults (your original)
        base_config = f"task_language={(language or 'eng').strip().lower()}|is_text_type=plain|os_task_file_format=json"

        # Optional 'sensitive' preset via env
        preset = (os.environ.get("AMS_AENEAS_PRESET") or "").strip().lower()
        if preset == "sensitive":
            base_config = _build_aeneas_sensitive_config(language)

        # Append any explicit config provided by caller (takes precedence for later keys)
        task_config = _merge_config(base_config, config)
        logger.debug(f"Aeneas config: {task_config}")

        # Command
        cmd = [
            python_exe,
            "-m",
            "aeneas.tools.execute_task",
            audio_path,
            text_file,
            task_config,
            output_file,
        ]

        logger.debug(f"Running command: {' '.join(cmd)}")

        try:
            result = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                timeout=max(60, timeout_sec),  # minimum guard
                cwd=temp_dir,
            )

            if result.returncode != 0:
                logger.error(f"Aeneas failed with exit code {result.returncode}")
                if result.stdout:
                    logger.error(f"STDOUT: {result.stdout}")
                if result.stderr:
                    logger.error(f"STDERR: {result.stderr}")
                raise RuntimeError(f"Aeneas alignment failed: {result.stderr or 'unknown error'}")

            if not os.path.exists(output_file):
                raise RuntimeError("Aeneas did not produce output file")

            with open(output_file, "r", encoding="utf-8") as f:
                alignment_data = json.load(f)

            fragments = []
            if isinstance(alignment_data, dict) and "fragments" in alignment_data:
                for fragment in alignment_data["fragments"]:
                    try:
                        begin = float(fragment.get("begin", 0))
                        end = float(fragment.get("end", 0))
                    except (TypeError, ValueError):
                        continue
                    if end >= begin:
                        fragments.append({"begin": begin, "end": end})

            text_digest = compute_text_digest(text_lines)

            return {
                "fragments": fragments,
                "text_digest": text_digest,
                "tool": {
                    "python": get_python_version(),
                    "aeneas": get_aeneas_version(),
                },
                "counts": {
                    "lines": len(text_lines),
                    "fragments": len(fragments),
                },
                # Optional: echo config used for observability (comment out if noisy)
                "config_used": task_config,
            }

        except subprocess.TimeoutExpired:
            raise RuntimeError(f"Aeneas alignment timed out after {timeout_sec} seconds")
        except Exception as e:
            logger.error(f"Alignment error: {str(e)}")
            raise


def validate_aeneas_installation() -> Dict[str, Any]:
    """
    Validate that Aeneas is properly installed and working.
    
    Returns:
        Dict with validation results and version info
    """
    # Use same logic as run_aeneas_alignment for finding Python executable
    python_exe = os.environ.get('AENEAS_PYTHON')
    if not python_exe:
        # Try to find system python with aeneas installed
        for candidate in ['python', 'python3', sys.executable]:
            try:
                result = subprocess.run([candidate, '-c', 'import aeneas'], 
                                      capture_output=True, timeout=5)
                if result.returncode == 0:
                    python_exe = candidate
                    break
            except (subprocess.TimeoutExpired, FileNotFoundError):
                continue
        else:
            python_exe = sys.executable  # fallback
    
    try:
        # Check Python version
        result = subprocess.run([python_exe, "-V"], capture_output=True, text=True, timeout=10)
        python_version = result.stdout.strip() if result.returncode == 0 else "unknown"
        
        # Check Aeneas import and version
        aeneas_check = subprocess.run([
            python_exe, "-c", 
            "import sys,aeneas;print(sys.version);print(getattr(aeneas,'__version__','unknown'))"
        ], capture_output=True, text=True, timeout=10)
        
        if aeneas_check.returncode == 0:
            lines = aeneas_check.stdout.strip().split('\n')
            aeneas_version = lines[-1] if lines else "unknown"
            aeneas_ok = True
        else:
            aeneas_version = "not-installed"
            aeneas_ok = False
        
        # Check execute_task availability
        task_help = subprocess.run([
            python_exe, "-m", "aeneas.tools.execute_task", "--help"
        ], capture_output=True, text=True, timeout=10)
        
        execute_task_ok = task_help.returncode == 0
        
        return {
            "ok": aeneas_ok and execute_task_ok,
            "python_executable": python_exe,
            "python_version": python_version,
            "aeneas_version": aeneas_version,
            "execute_task_available": execute_task_ok,
            "errors": [] if (aeneas_ok and execute_task_ok) else [
                "Aeneas import failed" if not aeneas_ok else None,
                "execute_task not available" if not execute_task_ok else None
            ]
        }
    
    except Exception as e:
        return {
            "ok": False,
            "python_executable": python_exe,
            "python_version": "unknown",
            "aeneas_version": "unknown", 
            "execute_task_available": False,
            "errors": [str(e)]
        }