# app.py
from __future__ import annotations

import os
import sys
import gc
import json
import time
import shlex
import logging
import tempfile
import traceback
import subprocess
from pathlib import Path
from typing import Optional, List, Dict, Tuple

# Make CUDA allocations less spiky on long decodes
os.environ.setdefault("PYTORCH_CUDA_ALLOC_CONF", "expandable_segments:True")

import numpy as np
import librosa
import soundfile as sf
import torch
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from huggingface_hub import login

# =============================================================================
# Configuration (via env vars)
# =============================================================================

# --- ASR (Step 1) ---
ASR_MODEL_NAME = os.getenv("ASR_MODEL", "nvidia/parakeet-tdt-0.6b-v3")
ASR_BEAM_SIZE = int(os.getenv("ASR_BEAM_SIZE", "12"))  # accuracy knob
ASR_BATCH = int(os.getenv("ASR_GPU_BATCH_SIZE", "8"))
ASR_FALLBACK_BATCH = int(os.getenv("ASR_GPU_FALLBACK_BATCH_SIZE", "1"))

# chunking for Step 1 ONLY (to avoid OOM)
ASR_PRECHUNK = os.getenv("ASR_PRECHUNK", "1").lower() not in ("0", "false", "no")
ASR_MIN_CHUNK_SEC = float(os.getenv("ASR_MIN_CHUNK_SEC", "60"))
ASR_MAX_CHUNK_SEC = float(os.getenv("ASR_MAX_CHUNK_SEC", "90"))
ASR_SILENCE_DB = float(os.getenv("ASR_SILENCE_DB", "35"))  # larger -> fewer segments

# --- NFA (Step 2) ---
# Use a *CTC* or *Hybrid-in-CTC-mode* model. To avoid stride/encoder pitfalls, default to pure CTC.
NFA_MODEL_NAME = os.getenv("NFA_MODEL", "stt_en_fastconformer_ctc_large")
# optional fallbacks if first is unavailable
NFA_MODEL_FALLBACKS = [
    "stt_en_conformer_ctc_large",
    "stt_en_fastconformer_ctc_small",
]
# Long-audio, streaming-ish alignment (keeps GPU memory reasonable)
NFA_USE_STREAMING = os.getenv("NFA_USE_STREAMING", "1").lower() not in ("0", "false", "no")
NFA_CHUNK_LEN = float(os.getenv("NFA_CHUNK_LEN", "1.6"))
NFA_TOTAL_BUFFER = float(os.getenv("NFA_TOTAL_BUFFER", "4.0"))
NFA_CHUNK_BATCH = int(os.getenv("NFA_CHUNK_BATCH", "32"))
NFA_BATCH = int(os.getenv("NFA_BATCH", "1"))
NFA_FEATURE_STRIDE = float(os.getenv("NFA_FEATURE_STRIDE", "0.01"))
NFA_MODEL_DOWNSAMPLE = int(os.getenv("NFA_MODEL_DOWNSAMPLE", "8"))
NFA_OUTPUT_TIMESTEP_DURATION = float(os.getenv("NFA_OUTPUT_TIMESTEP_DURATION", "0.08"))

# Force devices (leave None to auto)
NFA_TRANSCRIBE_DEVICE = os.getenv("NFA_TRANSCRIBE_DEVICE")  # "cuda" | "cpu" | None
NFA_VITERBI_DEVICE = os.getenv("NFA_VITERBI_DEVICE")        # "cuda" | "cpu" | None

# Optional: where your local NeMo repo lives (so we can import tools/nemo_forced_aligner programmatically)
NEMO_DIR = os.getenv("NEMO_DIR")  # e.g. "C:/tools/NeMo"

# Hugging Face token (for ASR download)
HF_TOKEN = os.getenv("HUGGINGFACE_TOKEN") or os.getenv("HF_TOKEN")

# =============================================================================
# Logging
# =============================================================================

def setup_logging() -> logging.Logger:
    logger = logging.getLogger()
    logger.setLevel(logging.INFO)
    logger.handlers.clear()
    ch = logging.StreamHandler(sys.stdout)
    ch.setLevel(logging.INFO)
    ch.setFormatter(logging.Formatter("%(asctime)s - %(levelname)s - %(message)s"))
    logger.addHandler(ch)
    return logger

logger = setup_logging()

# =============================================================================
# FastAPI
# =============================================================================

app = FastAPI(title="Hybrid Chunked ASR + Single-pass NFA", version="0.4.0")

class AlignRequest(BaseModel):
    audio_path: str
    beam_size: Optional[int] = None       # optional override for ASR beam
    nfa_model: Optional[str] = None       # optional override for NFA model (CTC strongly recommended)
    save_ass: bool = False                # also write ASS files (word-level)

class Token(BaseModel):
    w: str  # word/segment text
    t: float  # start sec
    d: float  # duration sec

class AlignResponse(BaseModel):
    transcript: str
    asr_model: str
    nfa_model: str
    words: List[Token]
    segments: List[Token]
    files: Dict[str, str]  # key -> path to generated files (ctm/ass/manifest)

# Back-compat legacy /asr endpoint types
class AsrRequest(BaseModel):
    audio_path: str
    model: Optional[str] = None
    language: str = "en"

class AsrToken(BaseModel):
    t: float
    d: float
    w: str

class LegacyAsrResponse(BaseModel):
    modelVersion: str
    tokens: List[AsrToken]

# =============================================================================
# Globals
# =============================================================================

_asr_model = None
_hf_ok = False

# =============================================================================
# Helpers / runtime utilities
# =============================================================================

def _hf_auth():
    """Hugging Face auth to allow downloading gated/pretrained weights."""
    global _hf_ok
    if _hf_ok:
        return
    if HF_TOKEN:
        try:
            login(token=HF_TOKEN, add_to_git_credential=True)
            _hf_ok = True
            logger.info("Hugging Face authentication OK")
        except Exception as e:
            logger.warning(f"HF auth failed: {e}")

def _absolute_path(p: str) -> str:
    return str(Path(p).expanduser().resolve())

def _prepare_chunks(audio_path: str,
                    min_chunk_sec: float,
                    max_chunk_sec: float,
                    silence_db: float) -> List[Dict[str, float]]:
    """
    Silence-aware chunking of the source audio for ASR ONLY.
    """
    y, sr = librosa.load(audio_path, sr=None)
    if y.ndim > 1:
        y = librosa.to_mono(y)

    total_samples = len(y)
    duration_sec = total_samples / sr if total_samples else 0.0
    if total_samples == 0:
        return []

    def _write_segment(arr, s, e, idx) -> Dict[str, float]:
        tmp = tempfile.NamedTemporaryFile(suffix=f"_chunk{idx}.wav", delete=False)
        sf.write(tmp.name, arr[s:e], sr)
        tmp.close()
        return {"path": tmp.name, "start_sec": s / sr, "end_sec": e / sr}

    if duration_sec <= max_chunk_sec:
        return [_write_segment(y, 0, total_samples, 0)]

    try:
        # Non-silent intervals [start, end] in samples
        non_silent = librosa.effects.split(y, top_db=silence_db)
    except Exception as exc:
        logger.warning(f"librosa.effects.split failed ({exc}); using fixed windows")
        step = int(max_chunk_sec * sr)
        out = []
        s = 0
        idx = 0
        while s < total_samples:
            e = min(total_samples, s + step)
            out.append(_write_segment(y, s, e, idx)); idx += 1
            s = e
        return out

    # candidate silence points between non-silent intervals
    silence_pts = {0, total_samples}
    for i in range(len(non_silent) - 1):
        s_end = non_silent[i][1]
        n_start = non_silent[i + 1][0]
        if n_start > s_end:
            silence_pts.add(int((s_end + n_start) / 2))

    silence_idx = sorted(silence_pts)
    silence_secs = [s / sr for s in silence_idx]

    segments: List[Tuple[int, int]] = []
    cur_start = 0
    cur_start_sec = 0.0
    target_len = (min_chunk_sec + max_chunk_sec) / 2.0

    while cur_start < total_samples:
        remain = duration_sec - cur_start_sec
        if remain <= max_chunk_sec:
            segments.append((cur_start, total_samples))
            break

        min_end_sec = cur_start_sec + min_chunk_sec
        max_end_sec = min(duration_sec, cur_start_sec + max_chunk_sec)
        candidates = [s for s in silence_secs if min_end_sec <= s <= max_end_sec]

        if candidates:
            target_sec = min(duration_sec, cur_start_sec + target_len)
            chosen_sec = min(candidates, key=lambda s: abs(s - target_sec))
        else:
            chosen_sec = max_end_sec

        chosen = int(round(chosen_sec * sr))
        if chosen <= cur_start:
            chosen = min(total_samples, cur_start + int(max_chunk_sec * sr))
            chosen_sec = chosen / sr

        segments.append((cur_start, chosen))
        cur_start = chosen
        cur_start_sec = chosen_sec

    out = []
    for idx, (s, e) in enumerate(segments):
        out.append(_write_segment(y, s, e, idx))
    return out

def _load_asr_model():
    """
    Load Parakeet-TDT ASR and set BEAM (timestamps OFF) with optional batched beam decoder.
    """
    global _asr_model
    if _asr_model is not None:
        _asr_model.eval()
        return _asr_model

    import nemo.collections.asr as nemo_asr
    device = "cuda" if torch.cuda.is_available() else "cpu"
    logger.info(f"Loading ASR model: {ASR_MODEL_NAME} on {device}")
    _asr_model = nemo_asr.models.ASRModel.from_pretrained(ASR_MODEL_NAME).to(device)
    _asr_model.eval()

    # Configure BEAM decoding with no alignment preservation
    try:
        import copy
        from omegaconf import OmegaConf, DictConfig

        decoding_cfg = copy.deepcopy(_asr_model.cfg.decoding)
        if isinstance(decoding_cfg, DictConfig):
            OmegaConf.set_struct(decoding_cfg, False)

        # MUST be "beam" for any RNNT beam variant
        decoding_cfg.strategy = "beam"

        # never ask alignments / timestamps during ASR
        if hasattr(decoding_cfg, "preserve_alignments"):
            decoding_cfg.preserve_alignments = False
        if hasattr(decoding_cfg, "compute_timestamps"):
            decoding_cfg.compute_timestamps = False

        beam_cfg = getattr(decoding_cfg, "beam", None)
        if beam_cfg is not None:
            if isinstance(beam_cfg, DictConfig):
                OmegaConf.set_struct(beam_cfg, False)

            # Pick the batched RNNT decoder HERE (NOT in strategy)
            # e.g. "malsd_batch" | "alsd" | "maes"
            beam_cfg.search_type = os.getenv("ASR_BEAM_SEARCH", "malsd_batch")

            # Width + core knobs
            beam_cfg.beam_size = int(os.getenv("ASR_BEAM_SIZE", "10"))
            beam_cfg.score_norm = True
            beam_cfg.return_best_hypothesis = True

            # Allow CUDA Graphs when supported
            if hasattr(beam_cfg, "allow_cuda_graphs"):
                beam_cfg.allow_cuda_graphs = True

            # (safety) never preserve alignments at beam level either
            if hasattr(beam_cfg, "preserve_alignments"):
                beam_cfg.preserve_alignments = False

        # Let NeMo fuse work across items for batched beam
        if hasattr(decoding_cfg, "fused_batch_size"):
            decoding_cfg.fused_batch_size = int(os.getenv("ASR_FUSED_BATCH_SIZE", "8"))

        _asr_model.change_decoding_strategy(decoding_cfg=decoding_cfg)
        try:
            _asr_model.cfg.decoding = decoding_cfg  # ensure transcribe() uses it
        except Exception:
            pass

        logger.info(
            "ASR set to beam_size=%s, search_type=%s, timestamps OFF",
            os.getenv("ASR_BEAM_SIZE", "10"), os.getenv("ASR_BEAM_SEARCH", "malsd_batch")
        )
    except Exception as e:
        logger.warning(f"Beam config failed; using defaults: {e}")

    return _asr_model


def _transcribe_chunked(audio_path: str, override_beam: Optional[int] = None) -> str:
    """
    Chunk the audio (if enabled) and transcribe chunks with timestamps OFF, then concatenate text.
    """
    _hf_auth()
    asr = _load_asr_model()

    # Optional request-time override for beam size
    if override_beam and override_beam != ASR_BEAM_SIZE:
        try:
            import copy
            from omegaconf import OmegaConf, DictConfig
            decoding_cfg = copy.deepcopy(asr.cfg.decoding)
            if isinstance(decoding_cfg, DictConfig):
                OmegaConf.set_struct(decoding_cfg, False)
            decoding_cfg.strategy = "batch"
            beam_cfg = getattr(decoding_cfg, "beam", None)
            if beam_cfg is not None:
                # pick a batched decoder here
                beam_cfg.search_type = os.getenv("ASR_BEAM_SEARCH", "malsd_batch")  # "malsd_batch" | "alsd" | "maes"
                beam_cfg.beam_size   = int(os.getenv("ASR_BEAM_SIZE", "10"))
                beam_cfg.score_norm  = True
                beam_cfg.return_best_hypothesis = True
                # graphs help a lot with batched decoders
                if hasattr(beam_cfg, "allow_cuda_graphs"):
                    beam_cfg.allow_cuda_graphs = True
            # never request alignments
            if hasattr(decoding_cfg, "compute_timestamps"):
                decoding_cfg.compute_timestamps = False
            if hasattr(decoding_cfg, "preserve_alignments"):
                decoding_cfg.preserve_alignments = False

            asr.change_decoding_strategy(decoding_cfg=decoding_cfg)
            try:
                asr.cfg.decoding = decoding_cfg
            except Exception:
                pass
            logger.info(f"Overrode beam_size to {override_beam}")
        except Exception as e:
            logger.warning(f"Failed to override beam size: {e}")

    # Prepare chunks for ASR
    if ASR_PRECHUNK:
        chunks = _prepare_chunks(audio_path, ASR_MIN_CHUNK_SEC, ASR_MAX_CHUNK_SEC, ASR_SILENCE_DB)
    else:
        chunks = [{"path": _absolute_path(audio_path), "start_sec": 0.0, "end_sec": 0.0}]

    if not chunks:
        return ""

    texts: List[str] = []
    for idx, ck in enumerate(chunks, 1):
        path = ck["path"]
        logger.info(f"ASR chunk {idx}/{len(chunks)}: {path}")

        kwargs = dict(return_hypotheses=True, timestamps=False, batch_size=ASR_BATCH)

        def _call():
            with torch.inference_mode():
                return asr.transcribe([path], **kwargs)

        try:
            hyps = _call()
        except torch.cuda.OutOfMemoryError:
            logger.warning("CUDA OOM during chunk ASR; retrying with smaller batch")
            kwargs["batch_size"] = ASR_FALLBACK_BATCH
            hyps = _call()

        hyp = hyps[0]
        if isinstance(hyp, list):
            hyp = hyp[0]
        text = (hyp.text or "").strip()
        texts.append(text)

        # Cleanup temp chunk
        if ASR_PRECHUNK and os.path.exists(path):
            try:
                os.remove(path)
            except Exception:
                pass

        gc.collect()
        if torch.cuda.is_available():
            torch.cuda.empty_cache()

    # Concatenate with simple sentence separators to help aligner
    full_text = ". ".join(t for t in texts if t)
    if full_text and not full_text.endswith((".", "!", "?")):
        full_text += "."
    return full_text

# ---------- NFA helpers ----------

def _find_align_py() -> Optional[Path]:
    """
    Locate NeMo's tools/nemo_forced_aligner/align.py with preference for the vendored copy.
    Search order:
      1) services/asr-nemo/vendor_nfa/align.py
      2) NEMO_DIR env (if given)
      3) installed nemo package path
    """
    vendored = Path(__file__).resolve().parent / "vendor_nfa" / "align.py"
    if vendored.exists():
        return vendored

    if NEMO_DIR:
        cand = Path(NEMO_DIR).expanduser().resolve() / "tools" / "nemo_forced_aligner" / "align.py"
        if cand.exists():
            return cand

    try:
        import nemo
        base = Path(nemo.__file__).resolve().parent
        candidates = [
            base / "tools" / "nemo_forced_aligner" / "align.py",
            base.parent / "nemo" / "tools" / "nemo_forced_aligner" / "align.py",  # when nested
        ]
        for c in candidates:
            if c.exists():
                return c
    except Exception:
        pass

    return None

def _import_nfa_programmatically():
    """
    Import AlignmentConfig and main() so we can call aligner in-process.
    We *prepend* the directory containing align.py to sys.path to satisfy its
    'from utils ...' absolute imports.
    """
    align_py = _find_align_py()
    if align_py is None:
        raise ImportError("NFA align.py not found in installed package or NEMO_DIR")

    nfadir = align_py.parent
    if str(nfadir) not in sys.path:
        sys.path.insert(0, str(nfadir))

    # Avoid package-relative import path issues ('from utils ...')
    from align import AlignmentConfig, main  # type: ignore
    return AlignmentConfig, main

def _compute_devices() -> Tuple[str, str]:
    if NFA_TRANSCRIBE_DEVICE in ("cuda", "cpu"):
        transcribe_dev = NFA_TRANSCRIBE_DEVICE
    else:
        transcribe_dev = "cuda" if torch.cuda.is_available() else "cpu"

    if NFA_VITERBI_DEVICE in ("cuda", "cpu"):
        viterbi_dev = NFA_VITERBI_DEVICE
    else:
        viterbi_dev = "cuda" if torch.cuda.is_available() else "cpu"

    return transcribe_dev, viterbi_dev

def _try_ctc_model(name: str) -> Optional[str]:
    """
    Return a CTC-capable model name if usable; otherwise None.
    """
    try:
        # quick sanity: name present; actual download is done by aligner
        return name
    except Exception:
        return None

def _select_nfa_model(request_name: Optional[str]) -> str:
    """
    Choose a CTC model for NFA: request override, else default, else fallbacks.
    """
    candidates: List[str] = []
    if request_name:
        candidates.append(request_name)
    candidates.append(NFA_MODEL_NAME)
    candidates.extend(NFA_MODEL_FALLBACKS)

    for nm in candidates:
        if not nm:
            continue
        if "ctc" in nm.lower():
            ok = _try_ctc_model(nm)
            if ok:
                if nm != candidates[0]:
                    logger.info(f"NFA using model: {nm}")
                return nm

    # If everything else fails, still return something (let aligner raise later)
    chosen = candidates[0]
    logger.warning(f"NFA model '{chosen}' may not be pure CTC; alignment may require extra overrides.")
    return chosen

def _write_manifest(audio_path: str, text: str, work_dir: Path) -> Path:
    mani = work_dir / "manifest.json"
    data = {"audio_filepath": _absolute_path(audio_path), "text": text}
    mani.write_text(json.dumps(data, ensure_ascii=False) + "\n", encoding="utf-8")
    return mani

def _run_nfa_inproc(manifest_path: Path, out_dir: Path, nfa_model: str, save_ass: bool) -> None:
    """
    Programmatic aligner call (preferred; avoids brittle CLI overrides).
    """
    AlignmentConfig, nfa_main = _import_nfa_programmatically()

    transcribe_dev, viterbi_dev = _compute_devices()
    # Keep config minimal and robust (NFA computes stride automatically for CTC models)
    cfg = AlignmentConfig(
        pretrained_name=nfa_model,
        model_path=None,
        manifest_filepath=str(manifest_path),
        output_dir=str(out_dir),
        align_using_pred_text=False,
        transcribe_device=transcribe_dev,
        viterbi_device=viterbi_dev,
        batch_size=NFA_BATCH,
        use_local_attention=True,
        audio_filepath_parts_in_utt_id=1,
        # Long audio knobs
        use_buffered_chunked_streaming=bool(NFA_USE_STREAMING),
        chunk_len_in_secs=float(NFA_CHUNK_LEN),
        total_buffer_in_secs=float(NFA_TOTAL_BUFFER),
        chunk_batch_size=int(NFA_CHUNK_BATCH),
        save_output_file_formats=(["ctm", "ass"] if save_ass else ["ctm"]),
        # ASS appearance knobs (only used if save_ass=True)
        # ass_file_config can be left default; service doesn't emit videos
    )
    cfg.feature_stride = float(NFA_FEATURE_STRIDE)
    cfg.model_downsample_factor = int(NFA_MODEL_DOWNSAMPLE)
    cfg.output_timestep_duration = float(NFA_OUTPUT_TIMESTEP_DURATION)
    nfa_main(cfg)

def _run_nfa_cli(manifest_path: Path, out_dir: Path, nfa_model: str, save_ass: bool) -> None:
    """
    Fallback: run align.py via subprocess (Hydra) mirroring the working PowerShell invocation.
    """
    align_py = _find_align_py()
    if not align_py:
        raise RuntimeError("Cannot locate NeMo NFA align.py for CLI fallback.")

    transcribe_dev, viterbi_dev = _compute_devices()
    save_formats = json.dumps(["ctm", "ass"] if save_ass else ["ctm"], separators=(",", ":"))

    hydra_overrides = [
        f'+feature_stride={NFA_FEATURE_STRIDE:g}',
        f'+model_downsample_factor={NFA_MODEL_DOWNSAMPLE}',
        f'+output_timestep_duration={NFA_OUTPUT_TIMESTEP_DURATION:g}',
        f'use_buffered_chunked_streaming={"true" if NFA_USE_STREAMING else "false"}',
        f'chunk_len_in_secs={NFA_CHUNK_LEN:g}',
        f'total_buffer_in_secs={NFA_TOTAL_BUFFER:g}',
        f'chunk_batch_size={NFA_CHUNK_BATCH}',
        f'batch_size={NFA_BATCH}',
        f'transcribe_device={transcribe_dev}',
        f'viterbi_device={viterbi_dev}',
        'use_local_attention=true',
        'audio_filepath_parts_in_utt_id=1',
        f'save_output_file_formats={save_formats}',
    ]

    cmd = [
        sys.executable, '-X', 'utf8', str(align_py),
        f'pretrained_name="{nfa_model}"',
        f'manifest_filepath="{str(manifest_path)}"',
        f'output_dir="{str(out_dir)}"',
        *hydra_overrides,
    ]

    env = os.environ.copy()
    env.setdefault("HYDRA_FULL_ERROR", "1")
    env.setdefault("PYTHONUTF8", "1")
    logger.info("Running NFA via subprocess: %s", " ".join(shlex.quote(c) for c in cmd))

    proc = subprocess.run(cmd, env=env, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
    if proc.returncode != 0:
        logger.error("NFA subprocess failed:\n%s", proc.stdout)
        raise RuntimeError(f"NFA subprocess failed with code {proc.returncode}")
    else:
        logger.debug("NFA output:\n%s", proc.stdout)


def _run_nfa(audio_path: str, transcript: str, request_model: Optional[str], save_ass: bool) -> Dict[str, Path]:
    """
    Build a 1-line manifest and run NFA (programmatic if possible, else CLI).
    """
    work_dir = Path(tempfile.mkdtemp(prefix="nfa_out_")).resolve()
    out_dir = work_dir / "out"
    out_dir.mkdir(parents=True, exist_ok=True)
    manifest = _write_manifest(audio_path, transcript, work_dir)

    nfa_model = _select_nfa_model(request_model)
    try:
        _run_nfa_inproc(manifest, out_dir, nfa_model, save_ass)
    except Exception as e_prog:
        logger.warning(f"NFA programmatic run failed ({e_prog}); trying subprocess.")
        _run_nfa_cli(manifest, out_dir, nfa_model, save_ass)

    # Expected outputs
    utt_id = Path(_absolute_path(audio_path)).stem.replace(" ", "-")
    files = {
        "root": out_dir,
        "ctm_tokens": out_dir / "ctm" / "tokens" / f"{utt_id}.ctm",
        "ctm_words": out_dir / "ctm" / "words" / f"{utt_id}.ctm",
        "ctm_segments": out_dir / "ctm" / "segments" / f"{utt_id}.ctm",
        "manifest_with_ctm_paths": out_dir / f"{manifest.stem}_with_ctm_paths.json",
    }
    if save_ass:
        files["ass_words"] = out_dir / "ass" / "words" / f"{utt_id}.ass"
        files["ass_segments"] = out_dir / "ass" / "segments" / f"{utt_id}.ass"

    return files

def _parse_ctm(ctm_path: Path) -> List[Token]:
    """
    Parse CTM lines: <utt_id> 1 <start> <dur> <text...>
    """
    toks: List[Token] = []
    if not ctm_path.exists():
        return toks
    with ctm_path.open("r", encoding="utf-8", errors="ignore") as f:
        for line in f:
            parts = line.strip().split()
            if len(parts) < 5:
                continue
            # ignore channel field (index 1)
            _, _, start, dur, *text_parts = parts
            try:
                t = float(start); d = float(dur)
            except ValueError:
                continue
            w = " ".join(text_parts)
            toks.append(Token(w=w, t=round(t, 3), d=round(d, 3)))
    return toks

# =============================================================================
# Routes
# =============================================================================

@app.get("/health")
def health():
    return {
        "status": "ok",
        "cuda": torch.cuda.is_available(),
        "device_count": torch.cuda.device_count() if torch.cuda.is_available() else 0,
        "asr_model": ASR_MODEL_NAME,
        "nfa_model_default": NFA_MODEL_NAME,
        "chunking": ASR_PRECHUNK,
        "ts": time.time(),
    }

@app.post("/align", response_model=AlignResponse)
def align(req: AlignRequest) -> AlignResponse:
    audio_path = _absolute_path(req.audio_path)
    if not Path(audio_path).exists():
        raise HTTPException(status_code=404, detail=f"Audio not found: {audio_path}")

    try:
        # Step 1: Chunked ASR with beam, timestamps OFF
        transcript = _transcribe_chunked(audio_path, override_beam=req.beam_size)
        logger.info("ASR transcript chars: %d", len(transcript))

        # Step 2: Single-pass NFA on FULL audio + full transcript
        files = _run_nfa(audio_path, transcript, req.nfa_model, save_ass=req.save_ass)

        # Parse CTMs (if present)
        words = _parse_ctm(files.get("ctm_words", Path()))
        segments = _parse_ctm(files.get("ctm_segments", Path()))

        return AlignResponse(
            transcript=transcript,
            asr_model=ASR_MODEL_NAME,
            nfa_model=req.nfa_model or NFA_MODEL_NAME,
            words=words,
            segments=segments,
            files={k: str(v) for k, v in files.items()},
        )
    except HTTPException:
        raise
    except Exception as e:
        logger.error("Alignment error: %s", traceback.format_exc())
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/asr", response_model=LegacyAsrResponse)
def legacy_asr(req: AsrRequest) -> LegacyAsrResponse:
    """
    Back-compat endpoint that returns word tokens. Internally runs full pipeline and maps words -> tokens.
    """
    ar = align(AlignRequest(audio_path=req.audio_path))
    toks = [AsrToken(t=w.t, d=w.d, w=w.w) for w in ar.words]
    return LegacyAsrResponse(modelVersion=ar.asr_model, tokens=toks)

# -----------------------------------------------------------------------------

if __name__ == "__main__":
    import uvicorn
    logger.info("Starting Hybrid Chunked ASR + Single-pass NFA on http://0.0.0.0:8000")
    uvicorn.run(app, host="0.0.0.0", port=8000)
