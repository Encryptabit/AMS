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


def run_aeneas_alignment(
    audio_path: str,
    text_lines: List[str], 
    language: str = "eng",
    timeout_sec: int = 600
) -> Dict[str, Any]:
    """
    Run Aeneas forced alignment on a single audio chunk with text lines.
    
    Args:
        audio_path: Path to audio file (WAV format)
        text_lines: List of text lines to align
        language: Language code (e.g., "eng", "fra")  
        timeout_sec: Maximum execution time
    
    Returns:
        Dict containing alignment fragments and metadata
    """
    if not text_lines:
        raise ValueError("text_lines cannot be empty")
    
    # Normalize paths for the environment we're running in
    audio_path = normalize_path(audio_path)
    
    if not os.path.exists(audio_path):
        raise FileNotFoundError(f"Audio file not found: {audio_path}")
    
    # Get Python executable from environment or use system default
    # Priority: AENEAS_PYTHON env var > system python > current python
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
    
    logger.info(f"Running alignment: {audio_path} with {len(text_lines)} lines")
    logger.debug(f"Python executable: {python_exe}")
    logger.debug(f"Language: {language}")
    
    with tempfile.TemporaryDirectory() as temp_dir:
        # Create text file with lines
        text_file = os.path.join(temp_dir, "text.txt")
        with open(text_file, 'w', encoding='utf-8') as f:
            for line in text_lines:
                f.write(line.strip() + '\n')
        
        # Output JSON file
        output_file = os.path.join(temp_dir, "alignment.json")
        
        # Build aeneas command
        task_config = f"task_language={language}|is_text_type=plain|os_task_file_format=json"
        
        cmd = [
            python_exe, "-m", "aeneas.tools.execute_task",
            audio_path,
            text_file,
            task_config,
            output_file
        ]
        
        logger.debug(f"Running command: {' '.join(cmd)}")
        
        try:
            result = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                timeout=timeout_sec,
                cwd=temp_dir
            )
            
            if result.returncode != 0:
                logger.error(f"Aeneas failed with exit code {result.returncode}")
                logger.error(f"STDOUT: {result.stdout}")
                logger.error(f"STDERR: {result.stderr}")
                raise RuntimeError(f"Aeneas alignment failed: {result.stderr}")
            
            # Read alignment results
            if not os.path.exists(output_file):
                raise RuntimeError("Aeneas did not produce output file")
            
            with open(output_file, 'r', encoding='utf-8') as f:
                alignment_data = json.load(f)
            
            # Extract fragments from Aeneas output
            fragments = []
            if 'fragments' in alignment_data:
                for fragment in alignment_data['fragments']:
                    fragments.append({
                        "begin": float(fragment.get('begin', 0)),
                        "end": float(fragment.get('end', 0))
                    })
            
            text_digest = compute_text_digest(text_lines)
            
            return {
                "fragments": fragments,
                "text_digest": text_digest,
                "tool": {
                    "python": get_python_version(),
                    "aeneas": get_aeneas_version()
                },
                "counts": {
                    "lines": len(text_lines),
                    "fragments": len(fragments)
                }
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