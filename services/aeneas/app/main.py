"""
Aeneas HTTP Service - FastAPI wrapper around aeneas.tools.execute_task

Provides REST endpoints for forced alignment of audio chunks with text lines.
"""
import os
import logging
from datetime import datetime
from typing import List, Optional
from pathlib import Path

from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field

from .exec_aeneas import run_aeneas_alignment, validate_aeneas_installation

# Setup logging
def setup_logging():
    """Configure logging to write to both console and file."""
    log_dir = Path("logs")
    log_dir.mkdir(exist_ok=True)
    
    logging.basicConfig(
        level=logging.INFO,
        format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
        handlers=[
            logging.StreamHandler(),
            logging.FileHandler(log_dir / "aeneas_service.log")
        ]
    )

setup_logging()
logger = logging.getLogger(__name__)

# Pydantic models
class AlignChunkRequest(BaseModel):
    chunk_id: str = Field(..., description="Unique identifier for this chunk")
    audio_path: str = Field(..., description="Path to audio file (WAV format)")
    lines: List[str] = Field(..., description="Text lines to align")
    language: str = Field(default="eng", description="Language code (e.g., 'eng', 'fra')")
    timeout_sec: Optional[int] = Field(default=600, description="Maximum execution time in seconds")

class Fragment(BaseModel):
    begin: float = Field(..., description="Start time in seconds")
    end: float = Field(..., description="End time in seconds")

class ToolInfo(BaseModel):
    python: str = Field(..., description="Python version")
    aeneas: str = Field(..., description="Aeneas version")

class AlignChunkResponse(BaseModel):
    chunk_id: str = Field(..., description="Chunk identifier from request")
    fragments: List[Fragment] = Field(..., description="Aligned time fragments")
    counts: dict = Field(..., description="Counts of lines and fragments")
    tool: ToolInfo = Field(..., description="Tool version information")
    generated_at: str = Field(..., description="ISO timestamp of generation")

class HealthResponse(BaseModel):
    ok: bool = Field(..., description="Overall health status")
    python_version: str = Field(..., description="Python version")
    aeneas_version: str = Field(..., description="Aeneas version")
    service: str = Field(default="aeneas-alignment", description="Service name")
    timestamp: str = Field(..., description="Current timestamp")

# FastAPI app
app = FastAPI(
    title="Aeneas Alignment Service",
    description="HTTP service for audio-text forced alignment using Aeneas",
    version="1.0.0"
)

@app.get("/")
async def root():
    """Root endpoint with basic service info."""
    return {
        "service": "aeneas-alignment",
        "version": "1.0.0", 
        "status": "running",
        "endpoints": ["/v1/health", "/v1/align-chunk"]
    }

@app.get("/v1/health", response_model=HealthResponse)
async def health_check():
    """
    Health check endpoint that validates Aeneas installation.
    
    Returns version information and installation status.
    """
    logger.info("Health check requested")
    
    try:
        validation = validate_aeneas_installation()
        
        return HealthResponse(
            ok=validation["ok"],
            python_version=validation["python_version"],
            aeneas_version=validation["aeneas_version"],
            timestamp=datetime.utcnow().isoformat()
        )
    
    except Exception as e:
        logger.error(f"Health check failed: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Health check failed: {str(e)}")

@app.post("/v1/align-chunk", response_model=AlignChunkResponse)
async def align_chunk(request: AlignChunkRequest):
    """
    Perform forced alignment on a single audio chunk with text lines.
    
    Takes an audio file and list of text lines, returns time-aligned fragments.
    """
    logger.info(f"Alignment requested for chunk {request.chunk_id}")
    logger.debug(f"Audio: {request.audio_path}")
    logger.debug(f"Lines: {len(request.lines)}")
    logger.debug(f"Language: {request.language}")
    
    if not request.lines:
        raise HTTPException(status_code=400, detail="Lines cannot be empty")
    
    if not os.path.exists(request.audio_path):
        raise HTTPException(status_code=404, detail=f"Audio file not found: {request.audio_path}")
    
    try:
        result = run_aeneas_alignment(
            audio_path=request.audio_path,
            text_lines=request.lines,
            language=request.language,
            timeout_sec=request.timeout_sec or 600
        )
        
        fragments = [Fragment(begin=f["begin"], end=f["end"]) for f in result["fragments"]]
        
        response = AlignChunkResponse(
            chunk_id=request.chunk_id,
            fragments=fragments,
            counts=result["counts"],
            tool=ToolInfo(python=result["tool"]["python"], aeneas=result["tool"]["aeneas"]),
            generated_at=datetime.utcnow().isoformat()
        )
        
        logger.info(f"Alignment completed for chunk {request.chunk_id}: {len(fragments)} fragments")
        return response
        
    except FileNotFoundError as e:
        logger.error(f"File not found: {str(e)}")
        raise HTTPException(status_code=404, detail=str(e))
    
    except ValueError as e:
        logger.error(f"Validation error: {str(e)}")
        raise HTTPException(status_code=400, detail=str(e))
    
    except RuntimeError as e:
        logger.error(f"Runtime error: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))
    
    except Exception as e:
        logger.error(f"Unexpected error: {str(e)}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Internal server error: {str(e)}")

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8082)