from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import List, Optional
import os
import gc
import torch
import traceback
import sys
import logging
import logging.handlers
from pathlib import Path
from datetime import datetime
from huggingface_hub import login

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

@app.post("/asr", response_model=AsrResponse)
async def transcribe_audio(request: AsrRequest):
    """
    ASR endpoint using NeMo for real transcription with timestamps.
    """
    global _model
    
    # Log the incoming request
    logging.info(f"ASR request received - Audio: {request.audio_path}, Model: {request.model}, Language: {request.language}")
    
    try:
        # Check if audio file exists
        if not os.path.exists(request.audio_path):
            logging.error(f"Audio file not found: {request.audio_path}")
            raise HTTPException(status_code=404, detail=f"Audio file not found: {request.audio_path}")
        
        # Ensure Hugging Face authentication
        if not _ensure_hf_auth():
            raise HTTPException(status_code=401, detail="Hugging Face authentication required. Please set HUGGINGFACE_TOKEN environment variable.")
        
        # Import NeMo (lazy import to avoid startup delays)
        try:
            import nemo.collections.asr as nemo_asr
        except ImportError as e:
            raise HTTPException(status_code=500, detail=f"Failed to import NeMo: {str(e)}")
        
        # Determine device
        device = "cuda" if torch.cuda.is_available() else "cpu"
        
        # Load model if not already loaded
        if _model is None:
            global _model_name
            logging.info("Loading ASR model...")
            try:
                # Try the latest Parakeet TDT v3 model first - supports timestamps and multilingual
                _model_name = "nvidia/parakeet-tdt-0.6b-v3"
                logging.info(f"Attempting to load model: {_model_name}")
                _model = nemo_asr.models.ASRModel.from_pretrained(_model_name).to(device)
                logging.info(f"Successfully loaded model: {_model_name} on {device}")
            except Exception as e:
                logging.warning(f"Failed to load Parakeet model: {e}")
                try:
                    # Fallback to QuartzNet - more stable and compatible
                    _model_name = "nvidia/stt_en_quartznet15x5"
                    logging.info(f"Attempting fallback to model: {_model_name}")
                    _model = nemo_asr.models.ASRModel.from_pretrained(_model_name).to(device)
                    logging.info(f"Successfully loaded fallback model: {_model_name} on {device}")
                except Exception as e2:
                    logging.warning(f"Failed to load QuartzNet model: {e2}")
                    try:
                        # Last fallback to conformer small
                        _model_name = "nvidia/stt_en_conformer_ctc_small"
                        logging.info(f"Attempting final fallback to model: {_model_name}")
                        _model = nemo_asr.models.ASRModel.from_pretrained(_model_name).to(device)
                        logging.info(f"Successfully loaded final fallback model: {_model_name} on {device}")
                    except Exception as e3:
                        logging.error(f"Failed to load any compatible model. Parakeet: {e}, QuartzNet: {e2}, Conformer: {e3}")
                        raise HTTPException(status_code=500, detail=f"Failed to load any compatible model. Errors: Parakeet: {str(e)}, QuartzNet: {str(e2)}, Conformer: {str(e3)}")
        else:
            # Ensure model is on correct device
            if next(_model.parameters()).device.type != device:
                _model = _model.to(device)
        
        # Sync CUDA if available
        if device == "cuda":
            torch.cuda.synchronize()
        
        # Perform transcription with timestamps
        logging.info(f"Starting transcription for: {request.audio_path}")
        start_time = datetime.now()
        
        output = _model.transcribe([request.audio_path], batch_size=16, return_hypotheses=True, timestamps=True)
        
        if device == "cuda":
            torch.cuda.synchronize()
        
        transcription_time = (datetime.now() - start_time).total_seconds()
        logging.info(f"Transcription completed in {transcription_time:.2f} seconds")
        
        hypo = output[0]
        logging.info(f"Transcription text length: {len(hypo.text)} characters")
        
        # Log raw NeMo output for debugging timestamp logic
        nemo_logger = logging.getLogger("nemo_raw")
        nemo_logger.info("=" * 80)
        nemo_logger.info(f"TRANSCRIPTION REQUEST: {request.audio_path}")
        nemo_logger.info("=" * 80)
        
        # Log hypothesis text and score
        nemo_logger.info(f"HYPOTHESIS TEXT: {hypo.text}")
        nemo_logger.info(f"HYPOTHESIS SCORE: {hypo.score if hasattr(hypo, 'score') else 'N/A'} (type: {type(hypo.score) if hasattr(hypo, 'score') else 'N/A'})")
        
        # Log all available attributes
        nemo_logger.info(f"HYPO ATTRIBUTES: {[attr for attr in dir(hypo) if not attr.startswith('_')]}")
        
        # Log timestamp data structure
        if hasattr(hypo, 'timestamp') and hypo.timestamp:
            nemo_logger.info(f"TIMESTAMP KEYS: {list(hypo.timestamp.keys())}")
            
            # Log word-level data
            if 'word' in hypo.timestamp:
                word_data = hypo.timestamp['word']
                nemo_logger.info(f"WORD COUNT: {len(word_data)}")
                nemo_logger.info("WORD DATA SAMPLE (first 5 entries):")
                for i, word_entry in enumerate(word_data[:5]):
                    nemo_logger.info(f"  Word {i}: {word_entry}")
                    nemo_logger.info(f"    Keys: {list(word_entry.keys()) if word_entry else 'Empty'}")
                
                if len(word_data) > 5:
                    nemo_logger.info(f"  ... and {len(word_data) - 5} more word entries")
            
            # Log segment-level data
            if 'segment' in hypo.timestamp:
                segment_data = hypo.timestamp['segment']
                nemo_logger.info(f"SEGMENT COUNT: {len(segment_data)}")
                nemo_logger.info("SEGMENT DATA SAMPLE (first 3 entries):")
                for i, segment_entry in enumerate(segment_data[:3]):
                    nemo_logger.info(f"  Segment {i}: {segment_entry}")
                    nemo_logger.info(f"    Keys: {list(segment_entry.keys()) if segment_entry else 'Empty'}")
                    
                if len(segment_data) > 3:
                    nemo_logger.info(f"  ... and {len(segment_data) - 3} more segment entries")
            
            # Log any other timestamp keys
            other_keys = [k for k in hypo.timestamp.keys() if k not in ['word', 'segment']]
            if other_keys:
                nemo_logger.info(f"OTHER TIMESTAMP KEYS: {other_keys}")
                for key in other_keys[:3]:  # Limit to first 3 to avoid spam
                    data = hypo.timestamp[key]
                    nemo_logger.info(f"  {key} ({len(data)} entries): {data[:2] if len(data) > 0 else 'Empty'}")
        else:
            nemo_logger.info("NO TIMESTAMP DATA AVAILABLE")
        
        nemo_logger.info("=" * 80)
        
        # Debug: Log NeMo output structure to understand available fields  
        logging.debug(f"hypo attributes: {[attr for attr in dir(hypo) if not attr.startswith('_')]}")
        if hasattr(hypo, 'timestamp') and hypo.timestamp:
            logging.debug(f"timestamp keys: {hypo.timestamp.keys()}")
            if 'word' in hypo.timestamp and hypo.timestamp['word']:
                sample_word = hypo.timestamp['word'][0] if hypo.timestamp['word'] else {}
                logging.debug(f"Sample word entry: {sample_word}")
                logging.debug(f"Word entry keys: {list(sample_word.keys()) if sample_word else 'No words'}")
        
        # Process NeMo output into word tokens directly
        tokens = []
        
        # Check if timestamps are available
        if hasattr(hypo, 'timestamp') and hypo.timestamp is not None and hypo.timestamp.get("word", []):
            # Use word-level timestamp information directly
            word_entries = hypo.timestamp.get("word", [])
            
            for word_entry in word_entries:
                word_start = float(word_entry.get("start", 0.0))
                word_end = float(word_entry.get("end", 0.0))
                        
                tokens.append(WordToken(
                    t=round(word_start, 2),
                    d=round(word_end - word_start, 2),
                    w="".join(word_entry.get("word", []))
                ))
        
        # Fallback: create basic tokens without detailed timestamps
        elif hypo.text:
            words = hypo.text.split()
            
            # Create mock tokens with estimated timing
            for i, word in enumerate(words):
                tokens.append(WordToken(
                    t=round(i * 0.5, 2),  # Estimate 0.5s per word
                    d=0.4,  # Estimate 0.4s duration per word
                    w=word
                ))
        
        # Cleanup
        gc.collect()
        if device == "cuda":
            torch.cuda.empty_cache()
        
        response = AsrResponse(
            modelVersion=_model_name or "unknown",
            tokens=tokens
        )
        
        logging.info(f"ASR processing completed - {len(tokens)} word tokens generated")
        return response
        
    except Exception as e:
        # Log the error with full traceback
        logging.error(f"Error processing audio {request.audio_path}: {str(e)}")
        logging.error(f"Full traceback:\n{traceback.format_exc()}")
        
        # Cleanup on error
        gc.collect()
        if torch.cuda.is_available():
            torch.cuda.empty_cache()
        
        error_detail = f"Error processing audio: {str(e)}\n{traceback.format_exc()}"
        raise HTTPException(status_code=500, detail=error_detail)

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
    
    uvicorn.run(app, host="0.0.0.0", port=8081)