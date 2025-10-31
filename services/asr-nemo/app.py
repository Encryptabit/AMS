from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import List, Optional
import os
os.environ.setdefault("PYTORCH_CUDA_ALLOC_CONF", "expandable_segments:True")
import gc
import asyncio
import numpy as np
import soundfile as sf
import librosa
import tempfile
import torch
import traceback
import sys
import logging
import logging.handlers
import io
from pathlib import Path
from datetime import datetime
from huggingface_hub import login

# Ensure stdout/stderr emit UTF-8 so logging can safely print diagnostic glyphs on Windows consoles
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")
else:
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding="utf-8", errors="replace")

# Set up logging configuration
def setup_logging():
    """Configure logging to write to both console and file with rotation."""
    # Create logs directory if it doesn't exist
    log_dir = Path("logs")
    log_dir.mkdir(exist_ok=True)
    
    # Configure root logger
    logger = logging.getLogger()
    logger.setLevel(logging.INFO)
    
    # Clear existing handlers to avoid duplicates
    logger.handlers.clear()
    
    # Console handler with color-friendly formatting
    console_handler = logging.StreamHandler(sys.stdout)
    console_handler.setLevel(logging.INFO)
    console_format = logging.Formatter(
        '%(asctime)s - %(name)s - %(levelname)s - %(message)s',
        datefmt='%Y-%m-%d %H:%M:%S'
    )
    console_handler.setFormatter(console_format)
    
    # File handler with rotation (10MB max, keep 5 files)
    log_file = log_dir / "asr_service.log"
    file_handler = logging.handlers.RotatingFileHandler(
        log_file,
        maxBytes=10*1024*1024,  # 10MB
        backupCount=5,
        encoding='utf-8'
    )
    file_handler.setLevel(logging.DEBUG)
    file_format = logging.Formatter(
        '%(asctime)s - %(name)s - %(levelname)s - %(funcName)s:%(lineno)d - %(message)s',
        datefmt='%Y-%m-%d %H:%M:%S'
    )
    file_handler.setFormatter(file_format)
    
    # Add handlers to root logger
    logger.addHandler(console_handler)
    logger.addHandler(file_handler)
    
    # Set up dedicated NeMo raw output logger
    nemo_logger = logging.getLogger("nemo_raw")
    nemo_logger.setLevel(logging.DEBUG)
    nemo_logger.handlers.clear()
    
    # NeMo raw output file handler (separate file for debugging)
    nemo_log_file = log_dir / "nemo-output.log"
    nemo_file_handler = logging.handlers.RotatingFileHandler(
        nemo_log_file,
        maxBytes=50*1024*1024,  # 50MB for raw data
        backupCount=3,
        encoding='utf-8'
    )
    nemo_file_handler.setLevel(logging.DEBUG)
    nemo_format = logging.Formatter(
        '%(asctime)s - NeMo Raw Output - %(message)s',
        datefmt='%Y-%m-%d %H:%M:%S'
    )
    nemo_file_handler.setFormatter(nemo_format)
    nemo_logger.addHandler(nemo_file_handler)
    
    # Set uvicorn logger to use our configuration
    uvicorn_logger = logging.getLogger("uvicorn")
    uvicorn_logger.handlers.clear()
    uvicorn_logger.addHandler(console_handler)
    uvicorn_logger.addHandler(file_handler)
    
    # Set fastapi logger
    fastapi_logger = logging.getLogger("fastapi")
    fastapi_logger.handlers.clear() 
    fastapi_logger.addHandler(file_handler)
    
    return logger

# Check Python version on startup
def check_python_version():
    """Check if we're running Python 3.10+ for better NeMo compatibility."""
    version_info = sys.version_info
    if version_info.major < 3 or (version_info.major == 3 and version_info.minor < 10):
        logging.warning(f"Running Python {version_info.major}.{version_info.minor}.{version_info.micro}")
        logging.warning("For best compatibility with modern NeMo models, Python 3.10+ is recommended.")
        logging.warning("Consider using the start_service.bat script to run with Python 3.12.")
    else:
        logging.info(f"Running Python {version_info.major}.{version_info.minor}.{version_info.micro} ✓")

# Initialize logging first
logger = setup_logging()

# Run version check
check_python_version()

app = FastAPI(title="ASR-NeMo Service", version="0.1.0")

class AsrRequest(BaseModel):
    audio_path: str
    model: Optional[str] = None
    language: str = "en"

class WordToken(BaseModel):
    t: float  # start time
    d: float  # duration
    w: str    # word

class AsrResponse(BaseModel):
    modelVersion: str
    tokens: List[WordToken]

_model = None
_model_name = None
_hf_authenticated = False

DEFAULT_GPU_BATCH_SIZE = int(os.getenv("ASR_GPU_BATCH_SIZE", "2"))
FALLBACK_GPU_BATCH_SIZE = int(os.getenv("ASR_GPU_FALLBACK_BATCH_SIZE", "1"))
_transcribe_semaphore = asyncio.Semaphore(int(os.getenv("ASR_MAX_CONCURRENT_JOBS", "1")))

def _ensure_hf_auth():
    """Ensure Hugging Face authentication is set up."""
    global _hf_authenticated
    
    if _hf_authenticated:
        return True
    
    # Try to get HF token from environment variable
    hf_token = os.getenv("HUGGINGFACE_TOKEN") or os.getenv("HF_TOKEN")
    
    if hf_token:
        try:
            login(token=hf_token, add_to_git_credential=True)
            _hf_authenticated = True
            logging.info("Successfully authenticated with Hugging Face")
            return True
        except Exception as e:
            logging.error(f"Failed to authenticate with Hugging Face: {e}")
            return False
    else:
        # Try to use existing token if already logged in
        try:
            from huggingface_hub import HfFolder
            token = HfFolder.get_token()
            if token:
                _hf_authenticated = True
                logging.info("Using existing Hugging Face authentication")
                return True
        except Exception:
            pass
        
        logging.warning("No Hugging Face token found. Please set HUGGINGFACE_TOKEN environment variable.")
        return False

def _normalize_word_entry(entry: dict, index: int) -> dict:
    """Normalize word entry from NeMo to our Token format"""
    return {
        "id": index,
        "text": "".join(entry.get("word", [])),
        "start": float(entry.get("start", 0.0)),
        "end": float(entry.get("end", 0.0)),
        "start_offset": int(entry.get("start_offset", 0)),
        "end_offset": int(entry.get("end_offset", 0)),
    }

def _normalize_segment_entry(entry: dict, index: int) -> dict:
    """Normalize segment entry from NeMo"""
    return {
        "id": index,
        "text": "".join(entry.get("segment", [])),
        "start": float(entry.get("start", 0.0)),
        "end": float(entry.get("end", 0.0)),
        "start_offset": int(entry.get("start_offset", 0)),
        "end_offset": int(entry.get("end_offset", 0)),
    }


def _load_model(device: str, nemo_asr_module, requested_model: Optional[str] = None):
    """Load the ASR model on the requested device if needed."""
    global _model, _model_name

    target_models = []
    if requested_model:
        target_models.append(requested_model)
    target_models.extend([
        "nvidia/parakeet-tdt-0.6b-v3",
        "nvidia/stt_en_quartznet15x5",
        "nvidia/stt_en_conformer_ctc_small",
    ])

    # If we already have a model loaded, check whether it satisfies the request
    if _model is not None:
        try:
            current_device = next(_model.parameters()).device.type
        except Exception:
            current_device = device

        if requested_model and _model_name != requested_model:
            logging.info(f"Requested model '{requested_model}' differs from loaded model '{_model_name}'. Reloading...")
            _cleanup_model()
        else:
            if current_device != device:
                logging.info(f"Moving model from {current_device} to {device}")
                _model = _model.to(device)
            _model.eval()
            return

    last_error: Optional[Exception] = None

    for candidate in target_models:
        if candidate is None:
            continue

        try:
            logging.info(f"Attempting to load model: {candidate}")
            model = nemo_asr_module.models.ASRModel.from_pretrained(candidate)
            model = model.to(device)
            model.eval()
            _model = model
            _model_name = candidate
            logging.info(f"Successfully loaded model: {_model_name} on {device}")
            return
        except Exception as exc:
            logging.warning(f"Failed to load model '{candidate}': {exc}")
            last_error = exc
            continue

    logging.error("Failed to load any compatible model. Last error: %s", last_error)
    raise HTTPException(
        status_code=500,
        detail=f"Failed to load requested model. Last error: {str(last_error)}"
    )


def _cleanup_model():
    """Cleanup model and free GPU memory"""
    global _model, _model_name
    try:
        if _model is not None:
            del _model
            _model = None
            _model_name = None
    except Exception:
        pass

    gc.collect()
    if torch.cuda.is_available():
        torch.cuda.synchronize()
        torch.cuda.empty_cache()
        torch.cuda.ipc_collect()
        try:
            torch.cuda.reset_peak_memory_stats()
        except Exception:
            pass


def _prepare_chunks(
    audio_path: str,
    min_chunk_sec: float = 60.0,
    max_chunk_sec: float = 90.0,
    silence_db: float = 45.0,
    min_pause_sec: float = 0.8,
):
    """Slice audio into manageable chunks aligned to low-energy regions."""
    y, sr = librosa.load(audio_path, sr=None)
    if y.ndim > 1:
        y = librosa.to_mono(y)

    total_samples = len(y)
    if total_samples == 0:
        return []

    duration_sec = total_samples / sr
    if duration_sec <= max_chunk_sec:
        temp_file = tempfile.NamedTemporaryFile(suffix='_chunk0.wav', delete=False)
        sf.write(temp_file.name, y, sr)
        temp_file.close()
        return [{"path": temp_file.name, "start_sec": 0.0, "end_sec": duration_sec}]

    segments = _derive_chunk_segments(y, sr, min_chunk_sec, max_chunk_sec, silence_db, min_pause_sec)
    chunk_infos = []
    for idx, (start_sample, end_sample) in enumerate(segments):
        temp_file = tempfile.NamedTemporaryFile(suffix=f'_chunk{idx}.wav', delete=False)
        sf.write(temp_file.name, y[start_sample:end_sample], sr)
        temp_file.close()
        chunk_infos.append({
            "path": temp_file.name,
            "start_sec": start_sample / sr,
            "end_sec": end_sample / sr
        })
    return chunk_infos


def _derive_chunk_segments(
    y: np.ndarray,
    sr: int,
    min_chunk_sec: float,
    max_chunk_sec: float,
    silence_db: float,
    min_pause_sec: float,
):
    total_samples = len(y)
    duration_sec = total_samples / sr
    if duration_sec <= max_chunk_sec:
        return [(0, total_samples)]

    try:
        non_silent = librosa.effects.split(y, top_db=silence_db)
    except Exception as exc:
        logging.warning("librosa.effects.split failed (%s); using fixed segmentation", exc)
        return _fallback_segments(total_samples, sr, max_chunk_sec)

    silence_points = {0, total_samples}
    min_pause_samples = int(min_pause_sec * sr)

    for i in range(len(non_silent) - 1):
        silence_start = non_silent[i][1]
        silence_end = non_silent[i + 1][0]
        if silence_end <= silence_start:
            continue

        if (silence_end - silence_start) >= min_pause_samples:
            silence_points.add(int((silence_start + silence_end) / 2))

    silence_samples = sorted(silence_points)
    silence_secs = [s / sr for s in silence_samples]

    segments = []
    chunk_start_sample = 0
    chunk_start_sec = 0.0
    target_len_sec = (min_chunk_sec + max_chunk_sec) / 2

    while chunk_start_sample < total_samples:
        remaining_sec = duration_sec - chunk_start_sec
        if remaining_sec <= max_chunk_sec:
            segments.append((chunk_start_sample, total_samples))
            break

        min_end_sec = chunk_start_sec + min_chunk_sec
        max_end_sec = min(duration_sec, chunk_start_sec + max_chunk_sec)
        candidate_secs = [s for s in silence_secs if min_end_sec <= s <= max_end_sec]

        if candidate_secs:
            target_sec = min(duration_sec, chunk_start_sec + target_len_sec)
            chosen_sec = min(candidate_secs, key=lambda s: abs(s - target_sec))
        else:
            chosen_sec = max_end_sec

        chosen_sample = int(round(chosen_sec * sr))
        if chosen_sample <= chunk_start_sample:
            chosen_sample = min(total_samples, chunk_start_sample + int(max_chunk_sec * sr))
            chosen_sec = chosen_sample / sr

        segments.append((chunk_start_sample, chosen_sample))
        chunk_start_sample = chosen_sample
        chunk_start_sec = chosen_sec

    if segments and segments[-1][1] < total_samples:
        segments[-1] = (segments[-1][0], total_samples)

    return segments


def _fallback_segments(total_samples: int, sr: int, max_chunk_sec: float):
    step_samples = int(max_chunk_sec * sr)
    segments = []
    start = 0
    while start < total_samples:
        end = min(total_samples, start + step_samples)
        segments.append((start, end))
        start = end
    return segments

@app.post("/asr", response_model=AsrResponse)
async def transcribe_audio(request: AsrRequest):
    """Serialized entry point that keeps GPU usage predictable by allowing one transcription at a time."""
    async with _transcribe_semaphore:
        return await _transcribe_impl(request)



async def _transcribe_impl(request: AsrRequest) -> AsrResponse:
    """Core ASR implementation with improved memory handling and timestamp extraction."""
    global _model

    logging.info(f"ASR request received - Audio: {request.audio_path}, Model: {request.model}, Language: {request.language}")

    chunk_infos = []
    try:
        if not os.path.exists(request.audio_path):
            logging.error(f"Audio file not found: {request.audio_path}")
            raise HTTPException(status_code=404, detail=f"Audio file not found: {request.audio_path}")

        if not _ensure_hf_auth():
            raise HTTPException(status_code=401, detail="Hugging Face authentication required. Please set HUGGINGFACE_TOKEN environment variable.")

        try:
            import nemo.collections.asr as nemo_asr
        except ImportError as e:
            raise HTTPException(status_code=500, detail=f"Failed to import NeMo: {str(e)}")

        device = "cuda" if torch.cuda.is_available() else "cpu"
        _load_model(device, nemo_asr, request.model)

        chunk_infos = _prepare_chunks(request.audio_path)
        if not chunk_infos:
            logging.warning("No content detected in audio; returning empty transcript")
            return AsrResponse(modelVersion=_model_name or "unknown", tokens=[])

        logging.info(f"Split audio into {len(chunk_infos)} chunk(s) for transcription")

        aggregated_tokens: List[WordToken] = []
        overall_start = datetime.now()

        for idx, chunk in enumerate(chunk_infos, start=1):
            chunk_duration = chunk["end_sec"] - chunk["start_sec"]
            logging.info(f"Chunk {idx}/{len(chunk_infos)}: offset={chunk['start_sec']:.2f}s length={chunk_duration:.2f}s on {device}")

            transcribe_kwargs = {
                "batch_size": DEFAULT_GPU_BATCH_SIZE,
                "return_hypotheses": True,
                "timestamps": True,
                "verbose": False
            }

            def _run_transcribe():
                with torch.inference_mode():
                    return _model.transcribe([chunk["path"]], **transcribe_kwargs)

            try:
                output = _run_transcribe()
            except torch.cuda.OutOfMemoryError:
                logging.warning("GPU OOM on chunk %s; retrying with batch size %s", idx, FALLBACK_GPU_BATCH_SIZE)
                if torch.cuda.is_available():
                    torch.cuda.synchronize()
                    torch.cuda.empty_cache()
                    torch.cuda.ipc_collect()

                transcribe_kwargs["batch_size"] = FALLBACK_GPU_BATCH_SIZE
                try:
                    output = _run_transcribe()
                except torch.cuda.OutOfMemoryError:
                    logging.error("GPU still OOM; moving model to CPU for chunk %s", idx)
                    if torch.cuda.is_available():
                        try:
                            _model = _model.to("cpu")
                            _model.eval()
                            device = "cpu"
                        except Exception as move_err:
                            logging.error("Failed to move model to CPU: %s", move_err)
                            raise HTTPException(status_code=500, detail="GPU out of memory and failed to move model to CPU.")
                    transcribe_kwargs["batch_size"] = 1
                    output = _run_transcribe()

            if device == "cuda":
                torch.cuda.synchronize()

            hypo = output[0]

            nemo_logger = logging.getLogger("nemo_raw")
            nemo_logger.info("=" * 80)
            nemo_logger.info(f"TRANSCRIPTION REQUEST CHUNK {idx}/{len(chunk_infos)}: offset={chunk['start_sec']:.2f}s path={chunk['path']}")
            nemo_logger.info("=" * 80)
            nemo_logger.info(f"HYPOTHESIS TEXT: {hypo.text}")
            nemo_logger.info(f"HYPOTHESIS SCORE: {hypo.score if hasattr(hypo, 'score') else 'N/A'} (type: {type(hypo.score) if hasattr(hypo, 'score') else 'N/A'})")
            nemo_logger.info(f"HYPO ATTRIBUTES: {[attr for attr in dir(hypo) if not attr.startswith('_')]}")

            if hasattr(hypo, 'timestamp') and hypo.timestamp:
                nemo_logger.info(f"TIMESTAMP KEYS: {list(hypo.timestamp.keys())}")
                if 'word' in hypo.timestamp:
                    word_data = hypo.timestamp['word']
                    nemo_logger.info(f"WORD COUNT: {len(word_data)}")
                    nemo_logger.info("WORD DATA SAMPLE (first 5 entries):")
                    for j, word_entry in enumerate(word_data[:5]):
                        nemo_logger.info(f"  Word {j}: {word_entry}")
                        nemo_logger.info(f"    Keys: {list(word_entry.keys()) if word_entry else 'Empty'}")
                    if len(word_data) > 5:
                        nemo_logger.info(f"  ... and {len(word_data) - 5} more word entries")
                if 'segment' in hypo.timestamp:
                    segment_data = hypo.timestamp['segment']
                    nemo_logger.info(f"SEGMENT COUNT: {len(segment_data)}")
                    nemo_logger.info("SEGMENT DATA SAMPLE (first 3 entries):")
                    for j, segment_entry in enumerate(segment_data[:3]):
                        nemo_logger.info(f"  Segment {j}: {segment_entry}")
                        nemo_logger.info(f"    Keys: {list(segment_entry.keys()) if segment_entry else 'Empty'}")
                other_keys = [k for k in hypo.timestamp.keys() if k not in ('word', 'segment')]
                if other_keys:
                    nemo_logger.info(f"OTHER TIMESTAMP KEYS: {other_keys}")
                    for key in other_keys[:3]:
                        data = hypo.timestamp[key]
                        nemo_logger.info(f"  {key} ({len(data)} entries): {data[:2] if len(data) > 0 else 'Empty'}")
            else:
                nemo_logger.info("NO TIMESTAMP DATA AVAILABLE")

            time_offset = chunk["start_sec"]
            if hasattr(hypo, 'timestamp') and hypo.timestamp is not None and hypo.timestamp.get("word", []):
                for word_entry in hypo.timestamp.get("word", []):
                    word_start = float(word_entry.get("start", 0.0))
                    word_end = float(word_entry.get("end", 0.0))
                    aggregated_tokens.append(WordToken(
                        t=round(time_offset + word_start, 2),
                        d=round(word_end - word_start, 2),
                        w="".join(word_entry.get("word", []))
                    ))
            elif hypo.text:
                for j, word in enumerate(hypo.text.split()):
                    aggregated_tokens.append(WordToken(
                        t=round(time_offset + j * 0.5, 2),
                        d=0.4,
                        w=word
                    ))

            gc.collect()
            if torch.cuda.is_available():
                torch.cuda.empty_cache()

        total_time = (datetime.now() - overall_start).total_seconds()
        logging.info(f"Transcription completed in {total_time:.2f} seconds across {len(chunk_infos)} chunk(s) on {device}")

        response = AsrResponse(
            modelVersion=_model_name or "unknown",
            tokens=aggregated_tokens
        )

        logging.info(f"ASR processing completed - {len(response.tokens)} word tokens generated")
        return response

    except HTTPException:
        raise
    except torch.cuda.OutOfMemoryError as oom:
        logging.error(f"Unrecoverable CUDA OOM: {oom}")
        gc.collect()
        if torch.cuda.is_available():
            torch.cuda.empty_cache()
        raise HTTPException(status_code=500, detail="CUDA out of memory during transcription. Please retry later.")
    except Exception as e:
        logging.error(f"Error processing audio {request.audio_path}: {str(e)}")
        logging.error("Full traceback: %s", traceback.format_exc())
        gc.collect()
        if torch.cuda.is_available():
            torch.cuda.empty_cache()
        error_detail = f"Error processing audio: {str(e)} | {traceback.format_exc()}"
        raise HTTPException(status_code=500, detail=error_detail)
    finally:
        for chunk in chunk_infos:
            try:
                os.remove(chunk["path"])
            except Exception as cleanup_err:
                logging.warning(f"Failed to remove temp chunk {chunk.get('path')}: {cleanup_err}")
@app.get("/health")
async def health_check():
    model_info = _model_name or "No model loaded"
    if _model is not None:
        device = next(_model.parameters()).device.type
        model_info += f" ({device})"
    
    cuda_info = {
        "available": torch.cuda.is_available(),
        "device_count": torch.cuda.device_count() if torch.cuda.is_available() else 0,
        "current_device": torch.cuda.current_device() if torch.cuda.is_available() else None,
        "device_name": torch.cuda.get_device_name() if torch.cuda.is_available() else None,
        "torch_version": torch.__version__
    }
    
    return {
        "status": "healthy", 
        "model": model_info, 
        "cuda": cuda_info,
        "hf_authenticated": _hf_authenticated
    }

@app.post("/auth/hf")
async def authenticate_hf():
    """Test Hugging Face authentication."""
    if _ensure_hf_auth():
        return {"status": "authenticated", "message": "Hugging Face authentication successful"}
    else:
        raise HTTPException(status_code=401, detail="Hugging Face authentication failed. Please set HUGGINGFACE_TOKEN environment variable.")

@app.post("/cleanup")
async def cleanup_model():
    """Cleanup the loaded model and free memory."""
    _cleanup_model()
    return {"status": "cleanup_complete", "message": "Model cleaned up and memory freed"}

@app.get("/memory")
async def memory_status():
    """Get current memory status."""
    result = {"cpu_memory": "N/A"}
    
    if torch.cuda.is_available():
        result["cuda_memory"] = {
            "allocated_gb": round(torch.cuda.memory_allocated() / 1024**3, 2),
            "reserved_gb": round(torch.cuda.memory_reserved() / 1024**3, 2),
            "device_count": torch.cuda.device_count()
        }
    else:
        result["cuda_memory"] = "CUDA not available"
    
    return result

if __name__ == "__main__":
    import uvicorn
    
    # Log service startup information
    logging.info("=" * 50)
    logging.info("ASR-NeMo Service Starting")
    logging.info("=" * 50)
    logging.info(f"Python version: {sys.version}")
    logging.info(f"PyTorch version: {torch.__version__}")
    logging.info(f"CUDA available: {torch.cuda.is_available()}")
    if torch.cuda.is_available():
        logging.info(f"CUDA device count: {torch.cuda.device_count()}")
        logging.info(f"CUDA device name: {torch.cuda.get_device_name()}")
    logging.info(f"Service log file: {Path('logs/asr_service.log').absolute()}")
    logging.info(f"NeMo debug log file: {Path('logs/nemo-output.log').absolute()}")
    logging.info("Server starting on http://0.0.0.0:8000")
    logging.info("Health check endpoint: http://0.0.0.0:8000/health")
    logging.info("=" * 50)
    
    uvicorn.run(app, host="0.0.0.0", port=8000)
