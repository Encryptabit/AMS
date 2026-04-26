#!/usr/bin/env bash
set -euo pipefail

SCRIPT_NAME="$(basename "$0")"
PROJECT_PATH="host/Ams.Tests/Ams.Tests.csproj"
CONFIGURATION="Debug"
NO_BUILD=0
DOTNET_BIN="${DOTNET_BIN:-dotnet}"

declare -a CHECK_NAMES=()
declare -a CHECK_FILTERS=()
declare -a CHECK_STATUSES=()
declare -a CHECK_EXIT_CODES=()
declare -a CHECK_DURATIONS_SEC=()
declare -a CHECK_LOG_PATHS=()

usage() {
    cat <<'USAGE'
Usage:
  bash scripts/verify_m008_s06_mobile_hardening.sh [options]

Options:
  --project <path>            Test project path (default: host/Ams.Tests/Ams.Tests.csproj)
  --configuration <name>      dotnet test configuration (default: Debug)
  --no-build                  Skip build when running test filters
  --help                      Show this usage text

Verification matrix:
  1) responsive-shell-prep   Shell/prep mobile responsive contract filters
  2) gesture-hardening       Proof gesture/touch interaction contract filters
  3) accessibility-fallbacks Proof fallback accessibility + CRX range contract filters
  4) persistence-seams       CRX/reviewed persistence seam contract filters
USAGE
}

add_check() {
    CHECK_NAMES+=("$1")
    CHECK_FILTERS+=("$2")
}

run_check() {
    local check_name="$1"
    local filter="$2"

    local log_file
    log_file="$(mktemp)"

    local start_sec=$SECONDS
    local -a command=("$DOTNET_BIN" test "$PROJECT_PATH" --nologo --configuration "$CONFIGURATION" --filter "$filter")

    if [[ "$NO_BUILD" -eq 1 ]]; then
        command+=(--no-build)
    fi

    set +e
    "${command[@]}" >"$log_file" 2>&1
    local exit_code=$?
    set -e

    local duration_sec=$((SECONDS - start_sec))

    CHECK_EXIT_CODES+=("$exit_code")
    CHECK_DURATIONS_SEC+=("$duration_sec")

    if [[ "$exit_code" -eq 0 ]]; then
        CHECK_STATUSES+=("pass")
        CHECK_LOG_PATHS+=("-")
        rm -f "$log_file"
        echo "[$SCRIPT_NAME] [$check_name] PASS (${duration_sec}s)"
    else
        CHECK_STATUSES+=("fail")
        CHECK_LOG_PATHS+=("$log_file")
        echo "[$SCRIPT_NAME] [$check_name] FAIL (${duration_sec}s)" >&2
        echo "[$SCRIPT_NAME] [$check_name] tail (last 60 lines):" >&2
        tail -n 60 "$log_file" >&2 || true
    fi
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --project)
            [[ $# -ge 2 ]] || {
                usage
                exit 64
            }
            PROJECT_PATH="$2"
            shift 2
            ;;
        --configuration)
            [[ $# -ge 2 ]] || {
                usage
                exit 64
            }
            CONFIGURATION="$2"
            shift 2
            ;;
        --no-build)
            NO_BUILD=1
            shift
            ;;
        --help|-h)
            usage
            exit 0
            ;;
        *)
            usage
            echo "Unknown argument: $1" >&2
            exit 64
            ;;
    esac
done

if ! command -v "$DOTNET_BIN" >/dev/null 2>&1; then
    echo "[$SCRIPT_NAME] dotnet CLI is required but was not found on PATH." >&2
    exit 127
fi

if [[ ! -f "$PROJECT_PATH" ]]; then
    echo "[$SCRIPT_NAME] test project does not exist: $PROJECT_PATH" >&2
    exit 2
fi

add_check "responsive-shell-prep" "FullyQualifiedName~StageShellLayoutTests|FullyQualifiedName~StageShellMobileContractTests|FullyQualifiedName~StageShellCompositionContractTests|FullyQualifiedName~WorkstationPrepMobileContractTests"
add_check "gesture-hardening" "FullyQualifiedName~ProofMobileInteractionContractTests|FullyQualifiedName~ProofGestureSelectionContractTests"
add_check "accessibility-fallbacks" "FullyQualifiedName~ProofMobileAccessibilityContractTests|FullyQualifiedName~ProofMobileCrxRangeContractTests"
add_check "persistence-seams" "FullyQualifiedName~ProofCrxBatchExportContractTests|FullyQualifiedName~WorkstationMobileHardeningContractTests"

for index in "${!CHECK_NAMES[@]}"; do
    run_check "${CHECK_NAMES[$index]}" "${CHECK_FILTERS[$index]}"
done

echo
echo "[$SCRIPT_NAME] Verification matrix summary"
printf '%-26s %-6s %-9s %s\n' "Check" "Status" "Duration" "Filter"
printf '%-26s %-6s %-9s %s\n' "--------------------------" "------" "---------" "------"

failure_count=0
for index in "${!CHECK_NAMES[@]}"; do
    if [[ "${CHECK_STATUSES[$index]}" != "pass" ]]; then
        failure_count=$((failure_count + 1))
    fi

    printf '%-26s %-6s %-9s %s\n' \
        "${CHECK_NAMES[$index]}" \
        "${CHECK_STATUSES[$index]}" \
        "${CHECK_DURATIONS_SEC[$index]}s" \
        "${CHECK_FILTERS[$index]}"
done

if [[ "$failure_count" -gt 0 ]]; then
    echo
    echo "[$SCRIPT_NAME] FAIL: ${failure_count} check(s) failed." >&2
    for index in "${!CHECK_NAMES[@]}"; do
        if [[ "${CHECK_STATUSES[$index]}" == "fail" ]]; then
            echo "[$SCRIPT_NAME] retained log for ${CHECK_NAMES[$index]}: ${CHECK_LOG_PATHS[$index]}" >&2
        fi
    done

    exit 1
fi

echo
echo "[$SCRIPT_NAME] PASS: all mobile hardening matrix checks passed."
