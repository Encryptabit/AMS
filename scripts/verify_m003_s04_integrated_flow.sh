#!/usr/bin/env bash
set -euo pipefail

SCRIPT_NAME="$(basename "$0")"
DEFAULT_EVIDENCE_OUT=".gsd/milestones/M003/slices/S04/S04-INTEGRATED-EVIDENCE-STUB.json"

CHECK_ONLY=0
WORKSPACE=""
CHAPTER_ID=""
CHAPTER_TITLE=""
MODEL_PATH=""
EVIDENCE_OUT="$DEFAULT_EVIDENCE_OUT"
TIMEOUT_SEC=45
PYTHON_BIN=""
RUN_STDOUT=""
RUN_STDERR=""
WORKSPACE_ABS=""
MODEL_PATH_ABS=""

REPO_ROOT="$(pwd)"

declare -a PHASE_NAMES=()
declare -a PHASE_STATUSES=()
declare -a PHASE_DETAILS=()

usage() {
    cat <<'USAGE'
Usage:
  bash scripts/verify_m003_s04_integrated_flow.sh [--check-only] [options]

Modes:
  --check-only                 Validate integrated continuity contracts and emit deterministic evidence stub.
                               No live workspace processing is executed.

Run-mode options (required unless --check-only):
  --workspace <path>           Workspace root path.
  --chapter-id <id>            Chapter stem/id (letters, numbers, ., _, - only).
  --model-path <path>          Pinned ASR model file path used for runtime-readiness evidence.

Optional options:
  --chapter-title <title>      Display title used for manual proof notes.
  --timeout-sec <seconds>      Timeout for external readiness commands (default: 45).
  --evidence-out <path>        Evidence JSON output path
                               (default: .gsd/milestones/M003/slices/S04/S04-INTEGRATED-EVIDENCE-STUB.json).
  --help                       Show usage.

Failure phases:
  prereq | readiness | playback-truth | rehydration | evidence
USAGE
}

sanitize_evidence_text() {
    local text="$1"
    local sanitized="$text"

    if [[ -n "$REPO_ROOT" ]]; then
        sanitized="${sanitized//${REPO_ROOT}/<repo>}"
    fi

    if [[ -n "$WORKSPACE_ABS" ]]; then
        sanitized="${sanitized//${WORKSPACE_ABS}/<workspace>}"
    fi

    if [[ -n "$MODEL_PATH_ABS" ]]; then
        sanitized="${sanitized//${MODEL_PATH_ABS}/<model>}"
    fi

    sanitized="${sanitized//$'\r'/ }"
    sanitized="${sanitized//$'\t'/ }"
    sanitized="${sanitized//$'\n'/ | }"

    printf '%s' "$sanitized"
}

add_phase_result() {
    local phase="$1"
    local status="$2"
    local detail
    detail="$(sanitize_evidence_text "$3")"

    PHASE_NAMES+=("$phase")
    PHASE_STATUSES+=("$status")
    PHASE_DETAILS+=("$detail")
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

    "$PYTHON_BIN" - "$phase_file" "$overall_status" "$summary" "$CHECK_ONLY" "$WORKSPACE" "$CHAPTER_ID" "$CHAPTER_TITLE" "$MODEL_PATH" "$REPO_ROOT" <<'PY'
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
repo_root = pathlib.Path(sys.argv[9]).resolve()


def _resolve(path_str: str) -> pathlib.Path:
    try:
        return pathlib.Path(path_str).expanduser().resolve(strict=False)
    except Exception:
        return pathlib.Path(path_str)


def redact_workspace(path_str: str | None) -> str | None:
    if not path_str:
        return None

    resolved = _resolve(path_str)
    try:
        rel = resolved.relative_to(repo_root)
        return rel.as_posix() if rel.as_posix() else "."
    except Exception:
        name = resolved.name or pathlib.Path(path_str).name
        if not name:
            return "<external-workspace>"
        return f"<external-workspace>/{name}"


def redact_model(path_str: str | None, workspace_str: str | None) -> str | None:
    if not path_str:
        return None

    resolved = _resolve(path_str)

    if workspace_str:
        workspace_resolved = _resolve(workspace_str)
        try:
            rel = resolved.relative_to(workspace_resolved)
            rel_text = rel.as_posix() if rel.as_posix() else "."
            return f"<workspace>/{rel_text}"
        except Exception:
            pass

    try:
        rel = resolved.relative_to(repo_root)
        return rel.as_posix() if rel.as_posix() else "."
    except Exception:
        name = resolved.name or pathlib.Path(path_str).name
        if not name:
            return "<external-model>"
        return f"<external-model>/{name}"


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
    "milestone": "M003",
    "slice": "S04",
    "task": "T03",
    "mode": "check-only" if check_only else "run",
    "overall_status": overall_status,
    "summary": summary,
    "inputs": {
        "workspace_ref": redact_workspace(workspace),
        "chapter_id": chapter_id or None,
        "chapter_title": chapter_title or None,
        "model_path_ref": redact_model(model_path, workspace),
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
    local safe_message
    safe_message="$(sanitize_evidence_text "$message")"

    add_phase_result "$phase" "fail" "$safe_message"

    local payload
    if payload="$(build_evidence_json "fail" "$safe_message")"; then
        printf '%s\n' "$payload"
        if [[ -n "$EVIDENCE_OUT" ]]; then
            mkdir -p "$(dirname "$EVIDENCE_OUT")" >/dev/null 2>&1 || true
            printf '%s\n' "$payload" >"$EVIDENCE_OUT" 2>/dev/null || true
        fi
    fi

    printf '[%s] %s\n' "$phase" "$safe_message" >&2
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

validate_optional_title() {
    local value="$1"

    if [[ -z "$value" ]]; then
        return 0
    fi

    local trimmed
    trimmed="$(echo "$value" | awk '{$1=$1};1')"
    [[ -n "$trimmed" ]]
}

validate_required_path_value() {
    local value="$1"
    local trimmed
    trimmed="$(echo "$value" | awk '{$1=$1};1')"
    [[ -n "$trimmed" ]]
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

validate_readiness_probe_output() {
    local stdout_text="$1"

    if [[ -z "$stdout_text" ]]; then
        return 1
    fi

    if [[ "$stdout_text" != *"Platform:"* ]]; then
        return 1
    fi

    if [[ "$stdout_text" != *"Repo root:"* ]]; then
        return 1
    fi

    return 0
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

require_file "prereq" "host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor" "proof editing page"
require_file "prereq" "host/Ams.Workstation.Server/Controllers/AudioController.cs" "corrected playback controller"
require_file "prereq" "host/Ams.Workstation.Server/Services/ProofPickupsSessionService.cs" "pickups session service"
require_file "prereq" "host/Ams.Workstation.Server/Services/BlazorWorkspace.cs" "workspace persistence service"
require_file "prereq" "host/Ams.Tests/Workstation/Proof/ProofEditingPlaybackSourceContractTests.cs" "proof editing playback contract tests"
require_file "prereq" "host/Ams.Tests/Workstation/Proof/ProofPickupsSessionServiceTests.cs" "pickups session diagnostics tests"
require_file "prereq" "host/Ams.Tests/Workstation/Proof/ProofPickupsSessionRestartResilienceTests.cs" "pickups restart resilience tests"
require_file "prereq" "host/Ams.Tests/Services/PickupCommitRevertReliabilityTests.cs" "pickup commit/revert reliability tests"
require_file "prereq" "host/Ams.Tests/Services/PickupArtifactLedgerTests.cs" "pickup artifact ledger tests"
require_file "prereq" ".gsd/milestones/M003/slices/S04/S04-INTEGRATED-EVIDENCE.md" "integrated evidence runbook"
require_file "prereq" "scripts/setup_ffmpeg.py" "ffmpeg readiness script"

if [[ "$CHECK_ONLY" -eq 0 ]]; then
    if [[ -z "$WORKSPACE" ]]; then
        fail_phase "prereq" "missing required argument --workspace <path>" 64
    fi

    if [[ ! -d "$WORKSPACE" ]]; then
        fail_phase "prereq" "workspace path does not exist: $WORKSPACE" 65
    fi

    WORKSPACE_ABS="$(cd "$WORKSPACE" >/dev/null 2>&1 && pwd -P || true)"

    if ! validate_chapter_id; then
        fail_phase "prereq" "invalid chapter id '$CHAPTER_ID' (expected letters, numbers, '.', '_' or '-')" 66
    fi

    if ! validate_required_path_value "$MODEL_PATH"; then
        fail_phase "prereq" "missing required argument --model-path <path>" 64
    fi

    if [[ ! -f "$MODEL_PATH" ]]; then
        fail_phase "prereq" "model path does not exist: $MODEL_PATH" 67
    fi

    MODEL_PATH_ABS="$($PYTHON_BIN - "$MODEL_PATH" <<'PY'
import pathlib
import sys

raw = sys.argv[1]
try:
    print(pathlib.Path(raw).expanduser().resolve(strict=False))
except Exception:
    print(raw)
PY
)"

    if ! validate_optional_title "$CHAPTER_TITLE"; then
        fail_phase "prereq" "malformed chapter title value (blank or whitespace-only)" 68
    fi
fi

add_phase_result "prereq" "pass" "required files, tooling, and run-mode inputs are valid"

# Phase: readiness
require_pattern "readiness" "scripts/setup_ffmpeg.py" "--check-only" "ffmpeg check-only CLI option contract"
require_pattern "readiness" "scripts/setup_ffmpeg.py" "FFmpeg binaries detected:" "ffmpeg readiness success output contract"
require_pattern "readiness" "scripts/setup_ffmpeg.py" "FFmpeg binaries are missing from expected paths:" "ffmpeg readiness failure output contract"

if [[ "$CHECK_ONLY" -eq 0 ]]; then
    if run_with_timeout "$TIMEOUT_SEC" "$PYTHON_BIN" "scripts/setup_ffmpeg.py" "--check-only"; then
        readiness_exit=0
    else
        readiness_exit=$?
    fi

    if [[ $readiness_exit -ne 0 ]]; then
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

    if ! validate_readiness_probe_output "$RUN_STDOUT"; then
        detail="malformed readiness probe output from setup_ffmpeg.py --check-only"
        if [[ -n "$RUN_STDOUT" ]]; then
            detail+=" | stdout: $RUN_STDOUT"
        fi
        if [[ -n "$RUN_STDERR" ]]; then
            detail+=" | stderr: $RUN_STDERR"
        fi
        fail_phase "readiness" "$detail" 69
    fi

    add_phase_result "readiness" "pass" "runtime-readiness probe command completed with parseable output"
else
    add_phase_result "readiness" "pass" "runtime-readiness script contracts validated (check-only mode)"
fi

# Phase: playback-truth
require_pattern "playback-truth" "host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor" "return $\"/api/audio/chapter/{Uri.EscapeDataString(ChapterName)}/corrected\";" "editing waveform corrected endpoint"
require_pattern "playback-truth" "host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor" "return $\"/api/audio/chapter/{Uri.EscapeDataString(ChapterName)}/corrected/peaks?pxPerSec={PeakPxPerSec}\";" "editing waveform corrected peaks endpoint"
require_pattern "playback-truth" "host/Ams.Workstation.Server/Controllers/AudioController.cs" "[HttpGet(\"chapter/{chapterName}/corrected\")]" "corrected playback endpoint route"
require_pattern "playback-truth" "host/Ams.Workstation.Server/Controllers/AudioController.cs" "[HttpGet(\"chapter/{chapterName}/corrected/peaks\")]" "corrected peaks endpoint route"
require_pattern "playback-truth" "host/Ams.Workstation.Server/Controllers/AudioController.cs" "new[] { (\"corrected\", audio.Corrected), (\"treated\", audio.Treated), (\"current\", currentContext) }" "deterministic corrected fallback order"
require_pattern "playback-truth" "host/Ams.Workstation.Server/Controllers/AudioController.cs" "(checked corrected, treated, current)" "explicit corrected fallback diagnostics"
require_pattern "playback-truth" "host/Ams.Tests/Workstation/Proof/ProofEditingPlaybackSourceContractTests.cs" "ChapterReview_PlaybackHelpers_TargetCorrectedAwareEndpoints" "contract test for corrected editing endpoints"
require_pattern "playback-truth" "host/Ams.Tests/Workstation/Proof/ProofEditingPlaybackSourceContractTests.cs" "AudioController_CorrectedResolver_DeclaresDeterministicFallbackAndFailClosedGuards" "contract test for corrected resolver guardrails"
require_pattern "playback-truth" "host/Ams.Tests/Workstation/Shell/ProofEditingContinuityTests.cs" "editing waveform peaks uses corrected endpoint" "shell continuity assertion for corrected peaks"

add_phase_result "playback-truth" "pass" "corrected editing playback + peaks contracts are present across page, controller, and tests"

# Phase: rehydration
require_pattern "rehydration" "host/Ams.Workstation.Server/Services/ProofPickupsSessionService.cs" "public ProofPickupsSessionSnapshot SyncToWorkspace(CancellationToken ct = default)" "session sync entrypoint"
require_pattern "rehydration" "host/Ams.Workstation.Server/Services/ProofPickupsSessionService.cs" "ArtifactLedgerReadError = ledgerReadError," "ledger read error surface"
require_pattern "rehydration" "host/Ams.Workstation.Server/Services/ProofPickupsSessionService.cs" "LastError = edlReadError ?? (chapterChanged ? null : _snapshot.LastError)," "EDL read error promotion into snapshot"
require_pattern "rehydration" "host/Ams.Workstation.Server/Services/ProofPickupsSessionService.cs" "EDL read failed for chapter" "EDL read failure diagnostic marker"
require_pattern "rehydration" "host/Ams.Workstation.Server/Services/ProofPickupsSessionService.cs" "Artifact ledger read failed for chapter" "artifact-ledger read failure diagnostic marker"
require_pattern "rehydration" "host/Ams.Workstation.Server/Services/ProofPickupsSessionService.cs" "Select an active chapter before pickup import or staging." "fail-closed chapter selection diagnostic"
require_pattern "rehydration" "host/Ams.Workstation.Server/Services/BlazorWorkspace.cs" "LastProjectStateLoadError =" "project-state load error field"
require_pattern "rehydration" "host/Ams.Workstation.Server/Services/BlazorWorkspace.cs" "Failed to load persisted project state" "project-state parse failure diagnostic"
require_pattern "rehydration" "host/Ams.Tests/Workstation/Proof/ProofPickupsSessionServiceTests.cs" "SyncToWorkspace_LedgerReadFailure_PreservesLastLedgerSnapshotWithReadError" "ledger read failure resilience test"
require_pattern "rehydration" "host/Ams.Tests/Workstation/Proof/ProofPickupsSessionServiceTests.cs" "SyncToWorkspace_EdlReadFailure_PreservesLastEdlSnapshotWithReadError" "EDL read failure resilience test"
require_pattern "rehydration" "host/Ams.Tests/Workstation/Proof/ProofPickupsSessionRestartResilienceTests.cs" "SyncToWorkspace_ReloadAndRestart_RehydratesMixedLifecycleSnapshotFromDurableState" "reload/restart convergence test"
require_pattern "rehydration" "host/Ams.Tests/Workstation/Proof/ProofPickupsSessionRestartResilienceTests.cs" "SyncToWorkspace_RestartWithMalformedProjectState_FailsClosedWithActionableDiagnostics" "malformed project-state fail-closed test"
require_pattern "rehydration" "host/Ams.Tests/Workstation/Proof/ProofPickupsSessionRestartResilienceTests.cs" "SyncToWorkspace_MalformedLedgerAfterBaseline_PreservesPriorLedgerSnapshotAndQuarantinesFile" "malformed ledger quarantine test"

add_phase_result "rehydration" "pass" "pickups session reload/restart + fail-closed diagnostics contracts are present"

# Phase: evidence
add_phase_result "evidence" "pass" "structured evidence payload generated"

if [[ "$CHECK_ONLY" -eq 1 ]]; then
    mode_label="check-only mode"
else
    mode_label="run mode"
fi

summary="S04 integrated editing↔pickups continuity checks passed (${mode_label})."
payload="$(build_evidence_json "pass" "$summary")" || fail_phase "evidence" "failed to build evidence payload" 70

printf '%s\n' "$payload"

if ! persist_evidence "$payload"; then
    fail_phase "evidence" "failed to write evidence payload to '$EVIDENCE_OUT'" 71
fi

echo "[$SCRIPT_NAME] PASS: evidence written to $EVIDENCE_OUT"