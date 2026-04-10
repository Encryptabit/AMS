#!/usr/bin/env bash
set -euo pipefail

SCRIPT_NAME="$(basename "$0")"
DEFAULT_EVIDENCE_OUT=".gsd/milestones/M002/slices/S04/S04-INTEGRATED-EVIDENCE-STUB.json"

CHECK_ONLY=0
WORKSPACE=""
CHAPTER_ID=""
CHAPTER_TITLE=""
MODEL_PATH=""
PROOF_SLUG=""
EVIDENCE_OUT="$DEFAULT_EVIDENCE_OUT"
TIMEOUT_SEC=45
PYTHON_BIN=""
RUN_STDOUT=""
RUN_STDERR=""

declare -a PHASE_NAMES=()
declare -a PHASE_STATUSES=()
declare -a PHASE_DETAILS=()

usage() {
    cat <<'USAGE'
Usage:
  bash scripts/verify_m002_s04_integrated_flow.sh [--check-only] [options]

Modes:
  --check-only                 Validate integrated continuity contracts and emit deterministic evidence stub.
                               No live workspace processing is executed.

Run-mode options (required unless --check-only):
  --workspace <path>           Workspace root path.
  --chapter-id <id>            Chapter stem/id (letters, numbers, ., _, - only).
  --model-path <path>          Pinned ASR model file path used for runtime-readiness evidence.

Optional options:
  --chapter-title <title>      Display title used for proof deep-link evidence.
  --proof-slug <slug>          Proof deep-link slug override (must not be blank or contain '/').
  --timeout-sec <seconds>      Timeout for external readiness commands (default: 45).
  --evidence-out <path>        Evidence JSON output path
                               (default: .gsd/milestones/M002/slices/S04/S04-INTEGRATED-EVIDENCE-STUB.json).
  --help                       Show usage.

Failure phases:
  prereq | readiness | proof-handoff | polish-scaffold | evidence
USAGE
}

add_phase_result() {
    PHASE_NAMES+=("$1")
    PHASE_STATUSES+=("$2")
    PHASE_DETAILS+=("$3")
}

select_python() {
    if command -v python3 >/dev/null 2>&1; then
        PYTHON_BIN="python3"
        return
    fi

    if command -v python >/dev/null 2>&1; then
        PYTHON_BIN="python"
        return
    fi

    echo "Neither 'python3' nor 'python' is available on PATH." >&2
    exit 127
}

build_evidence_json() {
    local overall_status="$1"
    local summary="$2"

    local phase_file
    phase_file="$(mktemp)"

    local i
    for i in "${!PHASE_NAMES[@]}"; do
        printf '%s\t%s\t%s\n' "${PHASE_NAMES[$i]}" "${PHASE_STATUSES[$i]}" "${PHASE_DETAILS[$i]}" >>"$phase_file"
    done

    "$PYTHON_BIN" - "$phase_file" "$overall_status" "$summary" "$CHECK_ONLY" "$WORKSPACE" "$CHAPTER_ID" "$CHAPTER_TITLE" "$MODEL_PATH" "$PROOF_SLUG" <<'PY'
import json
import pathlib
import sys

phase_file = pathlib.Path(sys.argv[1])
overall_status = sys.argv[2]
summary = sys.argv[3]
check_only = sys.argv[4] == "1"
workspace = sys.argv[5]
chapter_id = sys.argv[6]
chapter_title = sys.argv[7]
model_path = sys.argv[8]
proof_slug = sys.argv[9]

phases = []
if phase_file.exists():
    for line in phase_file.read_text(encoding="utf-8").splitlines():
        if not line:
            continue
        parts = line.split("\t", 2)
        while len(parts) < 3:
            parts.append("")
        phases.append({"phase": parts[0], "status": parts[1], "detail": parts[2]})

payload = {
    "milestone": "M002",
    "slice": "S04",
    "task": "T03",
    "mode": "check-only" if check_only else "run",
    "overall_status": overall_status,
    "summary": summary,
    "inputs": {
        "workspace": workspace or None,
        "chapter_id": chapter_id or None,
        "chapter_title": chapter_title or None,
        "model_path": model_path or None,
        "proof_slug": proof_slug or None,
    },
    "phases": phases,
}

print(json.dumps(payload, indent=2, sort_keys=True))
PY

    rm -f "$phase_file"
}

persist_evidence() {
    local json_payload="$1"

    local evidence_dir
    evidence_dir="$(dirname "$EVIDENCE_OUT")"

    if [[ -n "$evidence_dir" && "$evidence_dir" != "." ]]; then
        mkdir -p "$evidence_dir"
    fi

    printf '%s\n' "$json_payload" >"$EVIDENCE_OUT"
}

fail_phase() {
    local phase="$1"
    local message="$2"
    local exit_code="${3:-1}"

    add_phase_result "$phase" "fail" "$message"

    local payload
    if payload="$(build_evidence_json "fail" "$message")"; then
        printf '%s\n' "$payload"
        if [[ -n "$EVIDENCE_OUT" ]]; then
            mkdir -p "$(dirname "$EVIDENCE_OUT")" >/dev/null 2>&1 || true
            printf '%s\n' "$payload" >"$EVIDENCE_OUT" 2>/dev/null || true
        fi
    fi

    printf '[%s] %s\n' "$phase" "$message" >&2
    exit "$exit_code"
}

require_file() {
    local phase="$1"
    local file_path="$2"
    local detail="$3"

    if [[ ! -f "$file_path" ]]; then
        fail_phase "$phase" "missing required file: $file_path ($detail)" 2
    fi
}

require_pattern() {
    local phase="$1"
    local file_path="$2"
    local pattern="$3"
    local detail="$4"

    if ! grep -Fq -- "$pattern" "$file_path"; then
        fail_phase "$phase" "missing contract marker '$pattern' in $file_path ($detail)" 3
    fi
}

validate_chapter_id() {
    if [[ -z "$CHAPTER_ID" ]]; then
        return 1
    fi

    if [[ "$CHAPTER_ID" =~ ^[A-Za-z0-9._-]+$ ]]; then
        return 0
    fi

    return 1
}

validate_proof_slug() {
    local slug="$1"
    local trimmed
    trimmed="$(echo "$slug" | awk '{$1=$1};1')"

    if [[ -z "$trimmed" ]]; then
        return 1
    fi

    if [[ "$trimmed" == *"/"* ]]; then
        return 1
    fi

    if [[ "$trimmed" == *$'\n'* ]]; then
        return 1
    fi

    return 0
}

run_with_timeout() {
    local timeout_sec="$1"
    shift

    local stdout_file stderr_file
    stdout_file="$(mktemp)"
    stderr_file="$(mktemp)"

    set +e
    "$PYTHON_BIN" - "$timeout_sec" "$stdout_file" "$stderr_file" "$@" <<'PY'
import pathlib
import subprocess
import sys

timeout = float(sys.argv[1])
stdout_path = pathlib.Path(sys.argv[2])
stderr_path = pathlib.Path(sys.argv[3])
command = sys.argv[4:]

try:
    result = subprocess.run(command, capture_output=True, text=True, timeout=timeout)
    stdout_path.write_text(result.stdout or "", encoding="utf-8")
    stderr_path.write_text(result.stderr or "", encoding="utf-8")
    sys.exit(result.returncode)
except subprocess.TimeoutExpired as timeout_error:
    stdout_path.write_text(timeout_error.stdout or "", encoding="utf-8")
    stderr_path.write_text((timeout_error.stderr or "") + "\n[TIMEOUT]\n", encoding="utf-8")
    sys.exit(124)
PY
    local exit_code=$?
    set -e

    RUN_STDOUT="$(<"$stdout_file")"
    RUN_STDERR="$(<"$stderr_file")"
    rm -f "$stdout_file" "$stderr_file"

    return "$exit_code"
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --check-only)
            CHECK_ONLY=1
            shift
            ;;
        --workspace)
            [[ $# -ge 2 ]] || {
                usage
                exit 64
            }
            WORKSPACE="$2"
            shift 2
            ;;
        --chapter-id)
            [[ $# -ge 2 ]] || {
                usage
                exit 64
            }
            CHAPTER_ID="$2"
            shift 2
            ;;
        --chapter-title)
            [[ $# -ge 2 ]] || {
                usage
                exit 64
            }
            CHAPTER_TITLE="$2"
            shift 2
            ;;
        --model-path)
            [[ $# -ge 2 ]] || {
                usage
                exit 64
            }
            MODEL_PATH="$2"
            shift 2
            ;;
        --proof-slug)
            [[ $# -ge 2 ]] || {
                usage
                exit 64
            }
            PROOF_SLUG="$2"
            shift 2
            ;;
        --timeout-sec)
            [[ $# -ge 2 ]] || {
                usage
                exit 64
            }
            TIMEOUT_SEC="$2"
            shift 2
            ;;
        --evidence-out)
            [[ $# -ge 2 ]] || {
                usage
                exit 64
            }
            EVIDENCE_OUT="$2"
            shift 2
            ;;
        --help|-h)
            usage
            exit 0
            ;;
        *)
            usage
            printf 'Unknown argument: %s\n' "$1" >&2
            exit 64
            ;;
    esac
done

if ! [[ "$TIMEOUT_SEC" =~ ^[1-9][0-9]*$ ]]; then
    printf 'Invalid --timeout-sec value: %s\n' "$TIMEOUT_SEC" >&2
    exit 64
fi

select_python

# Phase: prereq
if ! command -v dotnet >/dev/null 2>&1; then
    fail_phase "prereq" "dotnet CLI is required but was not found on PATH." 127
fi

require_file "prereq" "host/Ams.Workstation.Server/Components/Navigation/StageRouteCatalog.cs" "stage-route catalog"
require_file "prereq" "host/Ams.Workstation.Server/Components/Pages/Prep/Index.razor" "prep shell page"
require_file "prereq" "host/Ams.Workstation.Server/Components/Pages/Polish/Index.razor" "polish scaffold page"
require_file "prereq" "host/Ams.Tests/Workstation/Shell/WorkstationIntegratedStageFlowTests.cs" "integrated shell continuity tests"
require_file "prereq" "host/Ams.Tests/Workstation/Prep/WorkstationPrepProofContinuityTests.cs" "prep-proof continuity tests"
require_file "prereq" "host/Ams.Tests/Workstation/Shell/ProofRouteCompatibilityTests.cs" "proof compatibility suite"
require_file "prereq" ".gsd/milestones/M002/slices/S04/S04-INTEGRATED-EVIDENCE.md" "integrated evidence runbook"
require_file "prereq" "scripts/setup_ffmpeg.py" "ffmpeg readiness script"

if [[ "$CHECK_ONLY" -eq 0 ]]; then
    if [[ -z "$WORKSPACE" ]]; then
        fail_phase "prereq" "missing required argument --workspace <path>" 64
    fi

    if [[ ! -d "$WORKSPACE" ]]; then
        fail_phase "prereq" "workspace path does not exist: $WORKSPACE" 65
    fi

    if ! validate_chapter_id; then
        fail_phase "prereq" "invalid chapter id '$CHAPTER_ID' (expected letters, numbers, '.', '_' or '-')" 66
    fi
fi

add_phase_result "prereq" "pass" "required files and baseline tooling are present"

# Phase: readiness
require_pattern "readiness" "host/Ams.Workstation.Server/Services/Prep/PrepRunSession.cs" "RuntimeReadinessStage = \"runtime_readiness\";" "prep session runtime-readiness stage marker"
require_pattern "readiness" "host/Ams.Workstation.Server/Services/Prep/PrepRuntimeReadinessSnapshot.cs" "PrepRuntimeModelProvenance" "model provenance snapshot contract"
require_pattern "readiness" "host/Ams.Workstation.Server/Services/Prep/PrepRuntimeReadinessProbe.cs" "--check-only" "ffmpeg check-only probe invocation"

if [[ "$CHECK_ONLY" -eq 0 ]]; then
    if [[ -z "$MODEL_PATH" ]]; then
        fail_phase "readiness" "missing required argument --model-path <path>" 64
    fi

    if [[ ! -f "$MODEL_PATH" ]]; then
        fail_phase "readiness" "model path does not exist: $MODEL_PATH" 67
    fi

    if ! run_with_timeout "$TIMEOUT_SEC" "$PYTHON_BIN" "scripts/setup_ffmpeg.py" "--check-only"; then
        readiness_exit=$?
        if [[ $readiness_exit -eq 124 ]]; then
            fail_phase "readiness" "timeout_marker: setup_ffmpeg.py --check-only exceeded ${TIMEOUT_SEC}s" 124
        fi

        detail="setup_ffmpeg.py --check-only failed"
        if [[ -n "$RUN_STDERR" ]]; then
            detail+=" | stderr: $RUN_STDERR"
        fi
        if [[ -n "$RUN_STDOUT" ]]; then
            detail+=" | stdout: $RUN_STDOUT"
        fi
        fail_phase "readiness" "$detail" "$readiness_exit"
    fi

    add_phase_result "readiness" "pass" "runtime-readiness probe contract validated and FFmpeg precheck command completed"
else
    add_phase_result "readiness" "pass" "runtime-readiness contract markers validated (check-only mode)"
fi

# Phase: proof-handoff
require_pattern "proof-handoff" "host/Ams.Workstation.Server/Components/Navigation/StageRouteCatalog.cs" "ProofChapterCompatibilityTemplate = \"/proof/{chapter}\"" "proof compatibility route template"
require_pattern "proof-handoff" "host/Ams.Workstation.Server/Components/Navigation/StageRouteCatalog.cs" "ProofChapterCanonicalTemplate = \"/proof/editing/{chapter}\"" "proof canonical chapter route template"
require_pattern "proof-handoff" "host/Ams.Workstation.Server/Components/Pages/Proof/Index.razor" "StageRouteCatalog.BuildProofChapterCompatibilityPath(chapterName);" "proof index chapter deep-link helper"
require_pattern "proof-handoff" "host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor" "StageRouteCatalog.BuildProofChapterCompatibilityPath(chapters[next])" "proof chapter keyboard continuity helper"

EFFECTIVE_PROOF_SLUG="$PROOF_SLUG"
if [[ -z "$EFFECTIVE_PROOF_SLUG" ]]; then
    if [[ -n "$CHAPTER_TITLE" ]]; then
        EFFECTIVE_PROOF_SLUG="$CHAPTER_TITLE"
    elif [[ -n "$CHAPTER_ID" ]]; then
        EFFECTIVE_PROOF_SLUG="$CHAPTER_ID"
    else
        EFFECTIVE_PROOF_SLUG="Chapter 01"
    fi
fi

if ! validate_proof_slug "$EFFECTIVE_PROOF_SLUG"; then
    fail_phase "proof-handoff" "malformed proof deep-link chapter slug '$EFFECTIVE_PROOF_SLUG'" 68
fi

add_phase_result "proof-handoff" "pass" "proof deep-link route helpers and chapter slug validation are coherent"

# Phase: polish-scaffold
require_pattern "polish-scaffold" "host/Ams.Workstation.Server/Components/Pages/Polish/Index.razor" "Polish is intentionally scaffold-only in the shared stage shell" "polish scaffold intent marker"
require_pattern "polish-scaffold" "host/Ams.Workstation.Server/Components/Pages/Polish/Index.razor" "/polish/legacy/pickups" "legacy pickup route link"
require_pattern "polish-scaffold" "host/Ams.Workstation.Server/Components/Pages/Polish/Index.razor" "/polish/legacy/batch" "legacy batch route link"
require_pattern "polish-scaffold" "host/Ams.Workstation.Server/Components/Navigation/StageRouteCatalog.cs" "ModuleIds.PolishScaffold" "polish scaffold module id contract"

add_phase_result "polish-scaffold" "pass" "polish scaffold shell markers and legacy route links are present"

# Phase: evidence
add_phase_result "evidence" "pass" "structured evidence payload generated"

summary="S04 integrated prep→proof→polish continuity checks passed (${CHECK_ONLY:+check-only mode})."
payload="$(build_evidence_json "pass" "$summary")" || fail_phase "evidence" "failed to build evidence payload" 69

printf '%s\n' "$payload"

if ! persist_evidence "$payload"; then
    fail_phase "evidence" "failed to write evidence payload to '$EVIDENCE_OUT'" 70
fi

echo "[$SCRIPT_NAME] PASS: evidence written to $EVIDENCE_OUT"
